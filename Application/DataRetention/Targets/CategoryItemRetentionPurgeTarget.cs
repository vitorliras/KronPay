using Domain.Entities.Configuration;
using Domain.Interfaces;
using Domain.Interfaces.Card;
using Domain.Interfaces.Transactions;

namespace Application.DataRetention.Targets;

public sealed class CategoryItemRetentionPurgeTarget : IRetentionPurgeTarget
{
    private readonly ICategoryItemRepository _categoryItems;
    private readonly ITransactionRepository _transactions;
    private readonly ICardPurchaseRepository _cardPurchases;

    public CategoryItemRetentionPurgeTarget(
        ICategoryItemRepository categoryItems,
        ITransactionRepository transactions,
        ICardPurchaseRepository cardPurchases)
    {
        _categoryItems = categoryItems;
        _transactions = transactions;
        _cardPurchases = cardPurchases;
    }

    public async Task<int> PurgeAsync(DateTime cutoff)
    {
        var candidates = await _categoryItems.GetDeactivatedOlderThanAsync(cutoff);
        if (candidates.Count == 0)
            return 0;

        var deletable = new List<CategoryItem>();

        foreach (var item in candidates)
        {
            var referencedByTransaction = await _transactions.ExistsByCategoryItemIdAsync(item.Id);
            var referencedByCardPurchase = await _cardPurchases.ExistsByCategoryItemIdAsync(item.Id);

            if (!referencedByTransaction && !referencedByCardPurchase)
                deletable.Add(item);
        }

        if (deletable.Count == 0)
            return 0;

        await _categoryItems.DeleteRangeAsync(deletable);
        return deletable.Count;
    }
}
