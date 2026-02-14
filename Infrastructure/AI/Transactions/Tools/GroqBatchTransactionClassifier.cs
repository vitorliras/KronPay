using Application.Abstractions.Import;
using Application.DTOs.Transactions;
using Domain.Interfaces;
using Infrastructure.AI.Prompts;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Infrastructure.AI.Transactions;

public sealed class GroqBatchTransactionClassifier : ITransactionAiBatchClassifier
{
    private readonly HttpClient _httpClient;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IConfiguration _config;

    public GroqBatchTransactionClassifier(
        HttpClient httpClient,
        ICategoryRepository categoryRepository,
        IConfiguration config)
    {
        _httpClient = httpClient;
        _categoryRepository = categoryRepository;
        _config = config;
    }

    public async Task<IReadOnlyList<TransactionAiSuggestion>> SuggestBatchAsync(
        int userId,
        IReadOnlyList<ImportedTransactionResponse> transactions,
        bool useAi = false)
    {
        var apiKey = _config["Groq:ApiKey"];
        var model = _config["Groq:Model"];

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("Groq ApiKey not configured.");

        if (string.IsNullOrWhiteSpace(model))
            throw new InvalidOperationException("Groq model not configured.");

        var categories = await _categoryRepository.GetAllAsync(userId);

        var prompt = TransactionBatchClassificationPrompt.Build(
            transactions,
            categories
        );

        var requestBody = new
        {
            model,
            temperature = 0,
            messages = new[]
            {
                new { role = "system", content = "You are a strict JSON API." },
                new { role = "user", content = prompt }
            }
        };

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "https://api.groq.com/openai/v1/chat/completions");

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);

        request.Content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _httpClient.SendAsync(request);
        var errorBody = await response.Content.ReadAsStringAsync();
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync();

        var groqResponse = JsonSerializer.Deserialize<GroqResponse>(
            body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        var json = groqResponse?
            .Choices?
            .FirstOrDefault()?
            .Message?
            .Content;

        if (string.IsNullOrWhiteSpace(json))
            return Array.Empty<TransactionAiSuggestion>();

        // 🔹 MESMA desserialização do Ollama
        var results = JsonSerializer.Deserialize<List<BatchAiResult>>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return results!
            .OrderBy(r => r.Index)
            .Select(r => new TransactionAiSuggestion(
                r.IsInvestment,
                r.Confidence,
                r.SuggestedCategoryId,
                r.SuggestedType))
            .ToList();
    }


    private sealed class GroqResponse
    {
        public List<Choice> Choices { get; set; } = new();
    }

    private sealed class Choice
    {
        public Message Message { get; set; } = default!;
    }

    private sealed class Message
    {
        public string Content { get; set; } = default!;
    }

    private sealed class BatchAiResult
    {
        public int Index { get; set; }
        public bool IsInvestment { get; set; }
        public decimal Confidence { get; set; }
        public int? SuggestedCategoryId { get; set; }
        public string? SuggestedType { get; set; }
    }
}
