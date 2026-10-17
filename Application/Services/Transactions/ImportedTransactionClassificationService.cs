using Application.DTOs.Transactions;
using Domain.Entities.Configuration;
using Domain.Services.Transactions;
using Microsoft.Extensions.Logging;
using Shared.AI;

namespace Application.Services.Transactions;

public sealed class ImportedTransactionClassificationService
{
    private const int BatchSize = 15;
    private const decimal MinimumConfidence = 0.70m;

    private readonly TransactionCategoryMemoryBuilder _memoryBuilder;
    private readonly ITransactionAiBatchClassifier _batchClassifier;
    private readonly ILogger<ImportedTransactionClassificationService> _logger;

    public ImportedTransactionClassificationService(
        TransactionCategoryMemoryBuilder memoryBuilder,
        ITransactionAiBatchClassifier batchClassifier,
        ILogger<ImportedTransactionClassificationService> logger)
    {
        _memoryBuilder = memoryBuilder;
        _batchClassifier = batchClassifier;
        _logger = logger;
    }

    public async Task<ImportedTransactionResponse?[]> ClassifyAsync(
        int userId,
        IReadOnlyList<ImportedTransactionResponse> importedTransactions,
        IEnumerable<Category> activeCategories,
        bool useAi)
    {
        var classified = new ImportedTransactionResponse?[importedTransactions.Count];

        var pending = await ApplyMemoryAsync(userId, activeCategories, importedTransactions, classified);

        var uniqueClassifications = await ClassifyPendingAsync(
            userId,
            useAi,
            importedTransactions,
            pending,
            classified);

        _logger.LogInformation(
            "Import classification: {Read} read, {FromMemory} from memory, {Unique} unique sent to the classifier.",
            importedTransactions.Count,
            importedTransactions.Count - pending.Count,
            uniqueClassifications);

        return classified;
    }

    private async Task<List<int>> ApplyMemoryAsync(
        int userId,
        IEnumerable<Category> categories,
        IReadOnlyList<ImportedTransactionResponse> importedTransactions,
        ImportedTransactionResponse?[] classified)
    {
        var memory = await _memoryBuilder.BuildAsync(userId, categories);
        var pending = new List<int>();

        for (var index = 0; index < importedTransactions.Count; index++)
        {
            var transaction = importedTransactions[index];

            var suggestion = transaction.CategoryId is null
                ? memory.Find(transaction.Description, transaction.Type)
                : null;

            if (suggestion is null)
            {
                pending.Add(index);
                continue;
            }

            classified[index] = transaction with
            {
                Type = suggestion.Type,
                CategoryId = suggestion.CategoryId,
                CategoryItemId = suggestion.CategoryItemId,
                ClassificationSource = ClassificationSources.Memory
            };
        }

        return pending;
    }

    private async Task<int> ClassifyPendingAsync(
        int userId,
        bool useAi,
        IReadOnlyList<ImportedTransactionResponse> importedTransactions,
        IReadOnlyList<int> pending,
        ImportedTransactionResponse?[] classified)
    {
        var groups = pending
            .GroupBy(index => GroupKey(importedTransactions[index], index))
            .Select(group => group.ToList())
            .ToList();

        foreach (var batch in groups.Chunk(BatchSize))
        {
            var representatives = batch
                .Select(group => importedTransactions[group[0]])
                .ToList();

            IReadOnlyList<TransactionAiSuggestion> suggestions;

            try
            {
                suggestions = await _batchClassifier.SuggestBatchAsync(userId, representatives, useAi);
            }
            catch
            {
                continue;
            }

            for (var position = 0; position < batch.Length; position++)
            {
                if (position >= suggestions.Count)
                    continue;

                foreach (var index in batch[position])
                    classified[index] = ApplySuggestion(importedTransactions[index], suggestions[position]);
            }
        }

        return groups.Count;
    }

    private static string GroupKey(ImportedTransactionResponse transaction, int index)
    {
        var normalized = TransactionDescriptionNormalizer.Normalize(transaction.Description);

        if (normalized.Length == 0)
            return $"#{index}";

        return $"{TransactionDescriptionNormalizer.IsOutflow(transaction.Type)}|{normalized}";
    }

    private static ImportedTransactionResponse ApplySuggestion(
        ImportedTransactionResponse original,
        TransactionAiSuggestion suggestion)
    {
        if (suggestion.Confidence < MinimumConfidence)
            return original;

        if (suggestion.IsInvestment)
        {
            var type = original.Type.Equals("I") ? "R" : "V";

            return original with
            {
                Type = type,
                CategoryId = suggestion.SuggestedCategoryId ?? original.CategoryId,
                ClassificationSource = suggestion.Source
            };
        }

        if (suggestion.SuggestedCategoryId is not null)
        {
            return original with
            {
                CategoryId = suggestion.SuggestedCategoryId.Value,
                ClassificationSource = suggestion.Source
            };
        }

        return original;
    }
}
