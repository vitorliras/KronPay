using Domain.Entities.Configuration;
using Domain.Interfaces;
using Domain.Interfaces.Card;
using Domain.Interfaces.Transactions;
using Domain.Services.Assistant;
using Domain.Services.Transactions;

namespace Application.Services.Transactions;

public sealed class TransactionCategoryMemoryBuilder
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICardPurchaseRepository _cardPurchaseRepository;
    private readonly ICategoryItemRepository _categoryItemRepository;
    private readonly IParameterFuzzyMatcher _fuzzyMatcher;

    public TransactionCategoryMemoryBuilder(
        ITransactionRepository transactionRepository,
        ICardPurchaseRepository cardPurchaseRepository,
        ICategoryItemRepository categoryItemRepository,
        IParameterFuzzyMatcher fuzzyMatcher)
    {
        _transactionRepository = transactionRepository;
        _cardPurchaseRepository = cardPurchaseRepository;
        _categoryItemRepository = categoryItemRepository;
        _fuzzyMatcher = fuzzyMatcher;
    }

    public async Task<TransactionCategoryMemory> BuildAsync(int userId, IEnumerable<Category> activeCategories)
    {
        var history = (await _transactionRepository.GetCategorizedHistoryAsync(userId))
            .Concat(await _cardPurchaseRepository.GetCategorizedHistoryAsync(userId))
            .ToList();
        var activeItems = await _categoryItemRepository.GetAllByUserIdAsync(userId);

        var categoryIds = activeCategories.Select(c => c.Id).ToHashSet();
        var itemCategoryById = activeItems.ToDictionary(i => i.Id, i => i.CategoryId);

        var entries = history
            .Where(item => categoryIds.Contains(item.CategoryId))
            .Select(item => (
                Key: (
                    Outflow: TransactionDescriptionNormalizer.IsOutflow(item.Type),
                    Description: TransactionDescriptionNormalizer.Normalize(item.Description)),
                Suggestion: new TransactionCategoryMemorySuggestion(
                    item.Type,
                    item.CategoryId,
                    ValidItemId(item, itemCategoryById)),
                item.Occurrences))
            .Where(x => x.Key.Description.Length > 0)
            .GroupBy(x => x.Key)
            .ToDictionary(group => group.Key, group => BuildEntry(group.ToList()));

        return new TransactionCategoryMemory(entries, _fuzzyMatcher);
    }

    private static int? ValidItemId(
        CategorizedTransactionHistoryItem item,
        IReadOnlyDictionary<int, int> itemCategoryById) =>
        item.CategoryItemId is { } itemId
        && itemCategoryById.TryGetValue(itemId, out var categoryId)
        && categoryId == item.CategoryId
            ? itemId
            : null;

    private static TransactionCategoryMemoryEntry BuildEntry(
        List<((bool Outflow, string Description) Key, TransactionCategoryMemorySuggestion Suggestion, int Occurrences)> rows)
    {
        var best = rows
            .GroupBy(r => r.Suggestion)
            .Select(g => (Suggestion: g.Key, Occurrences: g.Sum(r => r.Occurrences)))
            .OrderByDescending(x => x.Occurrences)
            .ThenBy(x => x.Suggestion.CategoryId)
            .ThenBy(x => x.Suggestion.CategoryItemId ?? int.MaxValue)
            .First();

        return new TransactionCategoryMemoryEntry(
            rows.Sum(r => r.Occurrences),
            best.Occurrences,
            best.Suggestion);
    }
}
