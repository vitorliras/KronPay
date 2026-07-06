using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Goals;
using Domain.Enums.Goals;
using Domain.Interfaces;
using Domain.Interfaces.Goals;
using Domain.Interfaces.Transactions;
using Domain.Services.Goals;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Goals;

public sealed class GetCategoryBudgetGoalStrategyUseCase
    : IUseCase<GetCategoryBudgetGoalStrategyRequest, CategoryGoalStrategyResponse>
{
    private const int HistoryMonths = 5;
    private const decimal SubcategorySignalThreshold = 0.5m;
    private const int FallbackTopCount = 3;

    private readonly ICategoryBudgetGoalRepository _goals;
    private readonly ITransactionRepository _transactions;
    private readonly ICategoryItemRepository _categoryItems;
    private readonly ISpendingTrendCalculator _trendCalculator;
    private readonly ICurrentUserService _currentUser;

    public GetCategoryBudgetGoalStrategyUseCase(
        ICategoryBudgetGoalRepository goals,
        ITransactionRepository transactions,
        ICategoryItemRepository categoryItems,
        ISpendingTrendCalculator trendCalculator,
        ICurrentUserService currentUser)
    {
        _goals = goals;
        _transactions = transactions;
        _categoryItems = categoryItems;
        _trendCalculator = trendCalculator;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<CategoryGoalStrategyResponse>> ExecuteAsync(GetCategoryBudgetGoalStrategyRequest request)
    {
        var userId = _currentUser.UserId;

        var goal = await _goals.GetByIdAsync(request.GoalId, userId);
        if (goal is null)
            return ResultEntity<CategoryGoalStrategyResponse>.Failure(MessageKeys.GoalNotFound);

        var now = DateTime.UtcNow;
        var currentMonthStart = new DateTime(now.Year, now.Month, 1);

        var currentMonthTransactions = (await _transactions.GetByMonthAsync(userId, now.Year, now.Month))
            .Where(t => t.CategoryId == goal.CategoryId && t.CodTypeTransaction == "E" && t.Status != "C")
            .ToList();

        var (groups, groupedBySubcategory) = await BuildGroupsAsync(goal.CategoryId, currentMonthTransactions);

        var trend = await BuildTrendAsync(userId, goal.CategoryId, currentMonthStart, currentMonthTransactions);

        var (suggestionKey, suggestionArgs) = BuildSuggestion(groups);

        var response = new CategoryGoalStrategyResponse(groupedBySubcategory, groups, trend, suggestionKey, suggestionArgs);

        return ResultEntity<CategoryGoalStrategyResponse>.Success(response, MessageKeys.OperationSuccess);
    }

    private async Task<(List<CategorySpendingGroupResponse> Groups, bool GroupedBySubcategory)> BuildGroupsAsync(
        int categoryId,
        List<Domain.Entities.Transactions.Transaction> currentMonthTransactions)
    {
        var withSubcategory = currentMonthTransactions.Count(t => t.CategoryItemId.HasValue);
        var groupedBySubcategory = currentMonthTransactions.Count > 0
            && (decimal)withSubcategory / currentMonthTransactions.Count >= SubcategorySignalThreshold;

        if (groupedBySubcategory)
        {
            var categoryItems = (await _categoryItems.GetAllAsync(categoryId))
                .ToDictionary(ci => ci.Id, ci => ci.Description);

            var subcategoryGroups = currentMonthTransactions
                .GroupBy(t => t.CategoryItemId)
                .Select(g => new CategorySpendingGroupResponse(
                    g.Key.HasValue && categoryItems.TryGetValue(g.Key.Value, out var name) ? name : "Outros",
                    g.Sum(t => t.Amount),
                    g.Count(),
                    g.Count() > 1))
                .OrderByDescending(g => g.Amount)
                .ToList();

            return (subcategoryGroups, groupedBySubcategory);
        }

        var topGroups = currentMonthTransactions
            .GroupBy(t => t.Description.Trim().ToUpperInvariant())
            .Select(g => new CategorySpendingGroupResponse(
                g.First().Description,
                g.Sum(t => t.Amount),
                g.Count(),
                g.Count() > 1))
            .OrderByDescending(g => g.Amount)
            .Take(FallbackTopCount)
            .ToList();

        return (topGroups, groupedBySubcategory);
    }

    private async Task<CategorySpendingTrendResponse> BuildTrendAsync(
        int userId,
        int categoryId,
        DateTime currentMonthStart,
        List<Domain.Entities.Transactions.Transaction> currentMonthTransactions)
    {
        var historyStart = currentMonthStart.AddMonths(-HistoryMonths);

        var pastTransactions = (await _transactions.GetByPeriodAsync(userId, historyStart, currentMonthStart))
            .Where(t => t.CategoryId == categoryId && t.CodTypeTransaction == "E" && t.Status != "C")
            .ToList();

        var monthlyTotals = new List<decimal>();
        for (var i = HistoryMonths; i >= 1; i--)
        {
            var month = currentMonthStart.AddMonths(-i);
            var total = pastTransactions
                .Where(t => t.TransactionDate.Year == month.Year && t.TransactionDate.Month == month.Month)
                .Sum(t => t.Amount);

            monthlyTotals.Add(total);
        }

        var historicalAverage = monthlyTotals.Count > 0 ? Math.Round(monthlyTotals.Average(), 2) : 0m;
        var currentPeriodSpent = currentMonthTransactions.Sum(t => t.Amount);

        var direction = _trendCalculator.ComputeDirection(monthlyTotals);

        return new CategorySpendingTrendResponse(currentPeriodSpent, historicalAverage, direction);
    }

    private static (string Key, IReadOnlyDictionary<string, string> Args) BuildSuggestion(
        List<CategorySpendingGroupResponse> groups)
    {
        var biggest = groups.FirstOrDefault();
        if (biggest is null)
            return (MessageKeys.NoSpendingSuggestionAvailable, new Dictionary<string, string>());

        var args = new Dictionary<string, string>
        {
            ["item"] = biggest.Label,
            ["amount"] = biggest.Amount.ToString("F2")
        };

        return (MessageKeys.CategorySpendingSuggestion, args);
    }
}
