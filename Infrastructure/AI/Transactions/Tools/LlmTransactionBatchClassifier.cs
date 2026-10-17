using System.Text.Json;
using Application.Abstractions.AI;
using Application.DTOs.Transactions;
using Domain.Interfaces;
using Infrastructure.AI.Prompts;
using Shared.AI;

namespace Infrastructure.AI.Transactions.Tools;

public sealed class LlmTransactionBatchClassifier : ITransactionAiBatchClassifier
{
    private const string SystemPrompt = "You are a strict JSON API.";

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private static readonly TransactionAiSuggestion Unclassified = new(
        IsInvestment: false,
        Confidence: 0m,
        SuggestedCategoryId: null,
        SuggestedType: null);

    private readonly IAiCompletionService _completionService;
    private readonly ICategoryRepository _categoryRepository;

    public LlmTransactionBatchClassifier(
        IAiCompletionService completionService,
        ICategoryRepository categoryRepository)
    {
        _completionService = completionService;
        _categoryRepository = categoryRepository;
    }

    public async Task<IReadOnlyList<TransactionAiSuggestion>> SuggestBatchAsync(
        int userId,
        IReadOnlyList<ImportedTransactionResponse> transactions,
        bool useAi = false)
    {
        if (transactions.Count == 0)
            return Array.Empty<TransactionAiSuggestion>();

        var categories = await _categoryRepository.GetAllAsync(userId);

        var prompt = TransactionBatchClassificationPrompt.Build(transactions, categories);

        var completion = await _completionService.CompleteAsync(
            new AiCompletionRequest(userId, AiFeatures.TransactionImport, SystemPrompt, prompt));

        var results = ParseResults(completion.Text);

        var suggestions = Enumerable.Repeat(Unclassified, transactions.Count).ToArray();

        foreach (var result in results)
        {
            var position = result.Index - 1;

            if (position < 0 || position >= suggestions.Length)
                continue;

            suggestions[position] = new TransactionAiSuggestion(
                result.IsInvestment,
                result.Confidence,
                result.SuggestedCategoryId,
                result.SuggestedType,
                ClassificationSources.Ai);
        }

        return suggestions;
    }

    private static List<BatchAiResult> ParseResults(string text)
    {
        var start = text.IndexOf('[');
        var end = text.LastIndexOf(']');

        if (start < 0 || end <= start)
            throw new InvalidOperationException("AI response did not contain a JSON array.");

        var json = text[start..(end + 1)];

        return JsonSerializer.Deserialize<List<BatchAiResult>>(json, JsonOptions)
            ?? throw new InvalidOperationException("AI response could not be parsed.");
    }
}
