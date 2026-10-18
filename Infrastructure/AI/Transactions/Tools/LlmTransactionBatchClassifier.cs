using System.Text.Json;
using Application.Abstractions.AI;
using Application.DTOs.Transactions;
using Domain.Entities.Configuration;
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
    private readonly ICategoryItemRepository _categoryItemRepository;

    public LlmTransactionBatchClassifier(
        IAiCompletionService completionService,
        ICategoryRepository categoryRepository,
        ICategoryItemRepository categoryItemRepository)
    {
        _completionService = completionService;
        _categoryRepository = categoryRepository;
        _categoryItemRepository = categoryItemRepository;
    }

    public async Task<IReadOnlyList<TransactionAiSuggestion>> SuggestBatchAsync(
        int userId,
        IReadOnlyList<ImportedTransactionResponse> transactions,
        bool useAi = false)
    {
        if (transactions.Count == 0)
            return Array.Empty<TransactionAiSuggestion>();

        var categories = await _categoryRepository.GetAllAsync(userId);
        var categoryItems = (await _categoryItemRepository.GetAllByUserIdAsync(userId)).ToList();

        var prompt = TransactionBatchClassificationPrompt.Build(transactions, categories, categoryItems);

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
                ClassificationSources.Ai,
                ValidItemId(result, categoryItems));
        }

        return suggestions;
    }

    private static int? ValidItemId(BatchAiResult result, IReadOnlyList<CategoryItem> categoryItems) =>
        result.SuggestedCategoryId is { } categoryId
        && result.SuggestedCategoryItemId is { } itemId
        && categoryItems.Any(i => i.Id == itemId && i.CategoryId == categoryId)
            ? itemId
            : null;

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
