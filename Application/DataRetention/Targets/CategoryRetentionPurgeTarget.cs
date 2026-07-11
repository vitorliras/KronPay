using Domain.Entities.Configuration;
using Domain.Interfaces;
using Domain.Interfaces.Card;
using Domain.Interfaces.Goals;
using Domain.Interfaces.Planning;
using Domain.Interfaces.Transactions;

namespace Application.DataRetention.Targets;

public sealed class CategoryRetentionPurgeTarget : IRetentionPurgeTarget
{
    private readonly ICategoryRepository _categories;
    private readonly ITransactionRepository _transactions;
    private readonly ICardPurchaseRepository _cardPurchases;
    private readonly ICategoryItemRepository _categoryItems;
    private readonly IPlannedCommitmentRepository _plannedCommitments;
    private readonly ICategoryBudgetGoalRepository _categoryBudgetGoals;

    public CategoryRetentionPurgeTarget(
        ICategoryRepository categories,
        ITransactionRepository transactions,
        ICardPurchaseRepository cardPurchases,
        ICategoryItemRepository categoryItems,
        IPlannedCommitmentRepository plannedCommitments,
        ICategoryBudgetGoalRepository categoryBudgetGoals)
    {
        _categories = categories;
        _transactions = transactions;
        _cardPurchases = cardPurchases;
        _categoryItems = categoryItems;
        _plannedCommitments = plannedCommitments;
        _categoryBudgetGoals = categoryBudgetGoals;
    }

    public async Task<int> PurgeAsync(DateTime cutoff)
    {
        var candidates = await _categories.GetDeactivatedOlderThanAsync(cutoff);
        if (candidates.Count == 0)
            return 0;

        var deletable = new List<Category>();

        foreach (var category in candidates)
        {
            var referenced =
                await _transactions.ExistsByCategoryIdAsync(category.Id) ||
                await _cardPurchases.ExistsByCategoryIdAsync(category.Id) ||
                await _categoryItems.ExistsByCategoryIdAsync(category.Id) ||
                await _plannedCommitments.ExistsByCategoryIdAsync(category.Id) ||
                await _categoryBudgetGoals.ExistsByCategoryIdAsync(category.Id);

            if (!referenced)
                deletable.Add(category);
        }

        if (deletable.Count == 0)
            return 0;

        await _categories.DeleteRangeAsync(deletable);
        return deletable.Count;
    }
}
