using Application.Services.Gamification;
using Domain.Enums.Gamification;
using Domain.Interfaces.Gamification;
using Domain.Interfaces.Goals;
using Domain.Interfaces.Transactions;
using Domain.Services.Gamification;

namespace Application.Services.Gamification.Evaluators;

public sealed class BudgetMissionEvaluator : IMissionEvaluator
{
    private const int RelapseStreakThreshold = 3;
    private const int NewCategoryWindowDays = 35;
    private const decimal WideCoverageThreshold = 0.7m;
    private const string HasBudgetMetKey = "HasBudgetMet";
    private const string ZeroRelapseMonthsKey = "ZeroRelapseMonths";
    private const string HasWideBudgetCoverageKey = "HasWideBudgetCoverage";

    private readonly ICategoryBudgetGoalRepository _budgetGoals;
    private readonly ITransactionRepository _transactions;
    private readonly IConsistencyCounterRepository _counters;

    public BudgetMissionEvaluator(
        ICategoryBudgetGoalRepository budgetGoals,
        ITransactionRepository transactions,
        IConsistencyCounterRepository counters)
    {
        _budgetGoals = budgetGoals;
        _transactions = transactions;
        _counters = counters;
    }

    public async Task<IReadOnlyList<MissionEvaluationResult>> EvaluateAsync(int userId, DateTime asOf)
    {
        var results = new List<MissionEvaluationResult>();

        var activeGoals = (await _budgetGoals.GetActiveAsync(userId)).ToList();
        if (activeGoals.Count == 0)
            return results;

        var monthStart = new DateTime(asOf.Year, asOf.Month, 1);
        var monthTransactions = (await _transactions.GetByPeriodAsync(userId, monthStart, asOf))
            .Where(t => t.Status != "C" && t.CodTypeTransaction == "E")
            .ToList();

        var usedExpenseCategoryIds = monthTransactions
            .Where(t => t.CategoryId.HasValue)
            .Select(t => t.CategoryId!.Value)
            .Distinct()
            .ToHashSet();

        var allOnTrack = true;
        var anyRelapse = false;

        foreach (var goal in activeGoals)
        {
            var spent = monthTransactions.Where(t => t.CategoryId == goal.CategoryId).Sum(t => t.Amount);
            var withinLimit = spent <= goal.MonthlyLimit;
            var counterKey = $"BudgetAdherence:CategoryId={goal.CategoryId}";
            var priorCounter = await _counters.GetAsync(userId, counterKey);
            var priorStreak = priorCounter?.CurrentStreak ?? 0;

            results.Add(new MissionEvaluationResult(MissionEventType.BudgetMonthClosed, goal.CategoryId, withinLimit));
            results.Add(new MissionEvaluationResult(MissionEventType.BudgetLimitExceeded, goal.CategoryId, !withinLimit));

            var isRelapse = !withinLimit && priorStreak >= RelapseStreakThreshold;
            results.Add(new MissionEvaluationResult(MissionEventType.BudgetRelapse, goal.CategoryId, isRelapse));

            var isNewCategory = goal.CreatedAt >= asOf.AddDays(-NewCategoryWindowDays);
            results.Add(new MissionEvaluationResult(
                MissionEventType.BudgetNewCategoryControlled, goal.CategoryId, isNewCategory && withinLimit));

            var newStreak = await ConsistencyCounterUpdater.UpdateStreakAsync(_counters, userId, counterKey, withinLimit);
            results.Add(new MissionEvaluationResult(
                MissionEventType.BudgetQuarterFlawless, goal.CategoryId, newStreak >= 3));

            if (withinLimit)
                await ConsistencyCounterUpdater.MarkOnceAsync(_counters, userId, HasBudgetMetKey);
            else
                allOnTrack = false;

            if (isRelapse)
                anyRelapse = true;
        }

        results.Add(new MissionEvaluationResult(MissionEventType.BudgetAllCategoriesOnTrack, null, allOnTrack));
        await ConsistencyCounterUpdater.UpdateStreakAsync(_counters, userId, ZeroRelapseMonthsKey, !anyRelapse);

        var coveredCategories = activeGoals.Select(g => g.CategoryId).ToHashSet();
        var coverageRatio = usedExpenseCategoryIds.Count == 0
            ? 0m
            : (decimal)usedExpenseCategoryIds.Count(coveredCategories.Contains) / usedExpenseCategoryIds.Count;

        var isWideCoverage = coverageRatio >= WideCoverageThreshold;
        results.Add(new MissionEvaluationResult(MissionEventType.BudgetWideCoverage, null, isWideCoverage));

        if (isWideCoverage)
            await ConsistencyCounterUpdater.MarkOnceAsync(_counters, userId, HasWideBudgetCoverageKey);

        return results;
    }
}
