using Application.Services.Gamification;
using Domain.Enums.Gamification;
using Domain.Enums.Goals;
using Domain.Interfaces;
using Domain.Interfaces.Gamification;
using Domain.Interfaces.Transactions;
using Domain.Services.Gamification;
using Domain.Services.Goals;

namespace Application.Services.Gamification.Evaluators;

public sealed class SpendingTrendMissionEvaluator : IMissionEvaluator
{
    private const int TrendHistoryMonths = 5;
    private const string TrendReversedCountKey = "TrendReversedCount";

    private readonly ICategoryRepository _categories;
    private readonly ITransactionRepository _transactions;
    private readonly ISpendingTrendCalculator _trendCalculator;
    private readonly IConsistencyCounterRepository _counters;

    public SpendingTrendMissionEvaluator(
        ICategoryRepository categories,
        ITransactionRepository transactions,
        ISpendingTrendCalculator trendCalculator,
        IConsistencyCounterRepository counters)
    {
        _categories = categories;
        _transactions = transactions;
        _trendCalculator = trendCalculator;
        _counters = counters;
    }

    public async Task<IReadOnlyList<MissionEvaluationResult>> EvaluateAsync(int userId, DateTime asOf)
    {
        var results = new List<MissionEvaluationResult>();

        var expenseCategories = (await _categories.GetAllAsync(userId))
            .Where(c => c.CodTypeTransaction == "E")
            .ToList();

        if (expenseCategories.Count == 0)
            return results;

        var currentMonthStart = new DateTime(asOf.Year, asOf.Month, 1);
        var historyStart = currentMonthStart.AddMonths(-TrendHistoryMonths);

        var historyTransactions = (await _transactions.GetByPeriodAsync(userId, historyStart, asOf))
            .Where(t => t.CodTypeTransaction == "E" && t.Status != "C")
            .ToList();

        foreach (var category in expenseCategories)
        {
            var categoryTransactions = historyTransactions.Where(t => t.CategoryId == category.Id).ToList();

            var monthlyTotals = new List<decimal>();
            for (var i = TrendHistoryMonths; i >= 1; i--)
            {
                var month = currentMonthStart.AddMonths(-i);
                var total = categoryTransactions
                    .Where(t => t.TransactionDate.Year == month.Year && t.TransactionDate.Month == month.Month)
                    .Sum(t => t.Amount);

                monthlyTotals.Add(total);
            }

            if (monthlyTotals.All(t => t == 0))
                continue;

            var direction = _trendCalculator.ComputeDirection(monthlyTotals);
            var isRising = direction == SpendingTrendDirection.Rising;
            var isUnderControl = !isRising;

            var wasRisingKey = $"WasRising:CategoryId={category.Id}";
            var wasRisingBefore = (await _counters.GetAsync(userId, wasRisingKey))?.CurrentStreak > 0;
            var reversed = wasRisingBefore && !isRising;

            await ConsistencyCounterUpdater.UpdateStreakAsync(_counters, userId, wasRisingKey, isRising);

            if (reversed)
                await ConsistencyCounterUpdater.IncrementOnlyAsync(_counters, userId, TrendReversedCountKey);

            results.Add(new MissionEvaluationResult(MissionEventType.SpendingCategoryUnderControl, category.Id, isUnderControl));
            results.Add(new MissionEvaluationResult(MissionEventType.SpendingCategoryRising, category.Id, isRising));
            results.Add(new MissionEvaluationResult(MissionEventType.SpendingTrendReversed, category.Id, reversed));
        }

        return results;
    }
}
