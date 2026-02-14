using Application.Abstractions.Import;
using Application.DTOs.Transactions;
using Domain.Interfaces;
using Infrastructure.AI.Prompts;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.Json;

namespace Infrastructure.AI.Transactions;

public sealed class OllamaTransactionClassifier : ITransactionAiClassifier
{
    private readonly HttpClient _httpClient;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IConfiguration _config;

    public OllamaTransactionClassifier(
        HttpClient httpClient,
        ICategoryRepository categoryRepository,
        IConfiguration config)
    {
        _httpClient = httpClient;
        _categoryRepository = categoryRepository;
        _config = config;
    }

    public async Task<TransactionAiSuggestion> SuggestAsync(
        int userId,
        string description,
        decimal amount,
        bool useAi = false)
    {
        var model = _config["Ollama:Model"];

        if (string.IsNullOrWhiteSpace(model))
            throw new InvalidOperationException("Ollama model not configured.");

        var categories = await _categoryRepository.GetAllAsync(userId);

        var prompt = TransactionClassificationPrompt.Build(
            description,
            amount,
            categories
        );

        var request = new OllamaRequest
        {
            Model = model,
            Prompt = prompt,
            Stream = false
        };

        var response = await _httpClient.PostAsJsonAsync(
            "/api/generate",
            request
        );

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException("Ollama request failed.");

        var body = await response.Content.ReadFromJsonAsync<OllamaResponse>();

        if (body is null || string.IsNullOrWhiteSpace(body.Response))
        {
            return new TransactionAiSuggestion(
                IsInvestment: false,
                Confidence: 0m,
                SuggestedCategoryId: null,
                SuggestedType: null
            );
        }

        var aiResult = JsonSerializer.Deserialize<AiResult>(
            body.Response,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        if (aiResult is null)
        {
            return new TransactionAiSuggestion(
                false, 0m, null, null
            );
        }

        return new TransactionAiSuggestion(
            aiResult.IsInvestment,
            aiResult.Confidence,
            aiResult.SuggestedCategoryId,
            aiResult.SuggestedType
        );
    }

    private sealed class OllamaRequest
    {
        public string Model { get; set; } = default!;
        public string Prompt { get; set; } = default!;
        public bool Stream { get; set; }
    }

    private sealed class OllamaResponse
    {
        public string Response { get; set; } = default!;
    }

    private sealed class AiResult
    {
        public bool IsInvestment { get; set; }
        public decimal Confidence { get; set; }
        public int? SuggestedCategoryId { get; set; }
        public string? SuggestedType { get; set; }
    }
}
