using Application.Abstractions.Import;
using Application.DTOs.Transactions;
using Domain.Interfaces;
using Infrastructure.AI.Prompts;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Infrastructure.AI.Transactions;

public sealed class OpenAiTransactionClassifier : ITransactionAiClassifier
{
    private readonly OpenAiClient _client;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IConfiguration _config;

    public OpenAiTransactionClassifier(
        OpenAiClient client,
        ICategoryRepository categoryRepository,
        IConfiguration config)
    {
        _client = client;
        _categoryRepository = categoryRepository;
        _config = config;
    }

    public async Task<TransactionAiSuggestion> SuggestAsync(
        int userId,
        string description,
        decimal amount,
        bool useAi = false)
    {
        var apiKey = _config["OpenAI:ApiKey"];
        var model = _config["OpenAI:Model"];

        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(model))
            throw new InvalidOperationException("OpenAI is not configured.");

        var categories = await _categoryRepository.GetAllAsync(userId);

        var prompt = TransactionClassificationPrompt.Build(
            description,
            amount,
            categories
        );

        var response = await _client.CompleteAsync(
            apiKey,
            model,
            prompt
        );

        var result = JsonSerializer.Deserialize<AiResult>(
            response,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        if (result is null)
        {
            return new TransactionAiSuggestion(
                IsInvestment: false,
                Confidence: 0m,
                SuggestedCategoryId: null,
                SuggestedType: null
            );
        }

        return new TransactionAiSuggestion(
            result.IsInvestment,
            result.Confidence,
            result.SuggestedCategoryId,
            result.SuggestedType
        );
    }

    private sealed class AiResult
    {
        public bool IsInvestment { get; set; }
        public decimal Confidence { get; set; }
        public int? SuggestedCategoryId { get; set; }
        public string? SuggestedType { get; set; }
    }
}
