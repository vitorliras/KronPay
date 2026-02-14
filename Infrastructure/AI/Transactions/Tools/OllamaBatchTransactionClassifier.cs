using Application.Abstractions.Import;
using Application.DTOs.Transactions;
using Domain.Interfaces;
using Infrastructure.AI.Prompts;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.Json;

namespace Infrastructure.AI.Transactions;

public sealed class OllamaBatchTransactionClassifier : ITransactionAiBatchClassifier
{
    private readonly HttpClient _httpClient;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IConfiguration _config;

    public OllamaBatchTransactionClassifier(
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

        var model = _config["Ollama:Model"];
        if (string.IsNullOrWhiteSpace(model))
            throw new InvalidOperationException("Ollama model not configured.");

        var categories = await _categoryRepository.GetAllAsync(userId);

        var prompt = TransactionBatchClassificationPrompt.Build(transactions, categories);

        var request = new
        {
            model,
            prompt,
            stream = false
        };

        var response = await _httpClient.PostAsJsonAsync("/api/generate", request);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<OllamaResponse>();

        var results = JsonSerializer.Deserialize<List<BatchAiResult>>(
            body!.Response,
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

    private sealed class BatchAiResult
    {
        public int Index { get; set; }
        public bool IsInvestment { get; set; }
        public decimal Confidence { get; set; }
        public int? SuggestedCategoryId { get; set; }
        public string? SuggestedType { get; set; }
    }
}
