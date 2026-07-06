using Application.Planning;
using Domain.Entities.Configuration;
using Domain.Enums.Goals;
using Domain.Enums.Notifications;
using Domain.Interfaces;
using Domain.Interfaces.Goals;
using Domain.Interfaces.Transactions;
using Domain.Services.Goals;
using Shared.Localization;

namespace Application.Notifications.Rules;

public sealed class FinancialIntelligenceNotificationRuleEvaluator : IFinancialIntelligenceNotificationRuleEvaluator
{
    private const int TrendHistoryMonths = 5;
    private const decimal HighSpendingWithoutBudgetThreshold = 1.5m;
    private const decimal SignificantIncreaseThreshold = 1.3m;
    private const decimal AboveAverageThreshold = 1.15m;
    private const decimal RecurringIncreaseThreshold = 1.15m;
    private const decimal SavingsThreshold = 0.9m;
    private const decimal NetWorthChangePercentThreshold = 20m;

    private readonly IProjectionRunner _projectionRunner;
    private readonly ICategoryRepository _categories;
    private readonly ICategoryBudgetGoalRepository _categoryBudgetGoals;
    private readonly ITransactionRepository _transactions;
    private readonly ISpendingTrendCalculator _trendCalculator;

    public FinancialIntelligenceNotificationRuleEvaluator(
        IProjectionRunner projectionRunner,
        ICategoryRepository categories,
        ICategoryBudgetGoalRepository categoryBudgetGoals,
        ITransactionRepository transactions,
        ISpendingTrendCalculator trendCalculator)
    {
        _projectionRunner = projectionRunner;
        _categories = categories;
        _categoryBudgetGoals = categoryBudgetGoals;
        _transactions = transactions;
        _trendCalculator = trendCalculator;
    }

    public async Task<IReadOnlyList<NotificationCandidate>> EvaluateAsync(int userId, DateTime today)
    {
        var candidates = new List<NotificationCandidate>();

        candidates.AddRange(await EvaluateNegativeBalanceAsync(userId, today));
        candidates.AddRange(await EvaluateCategoryTrendsAsync(userId, today));
        candidates.AddRange(await EvaluateRecurringExpensesAsync(userId, today));
        candidates.AddRange(await EvaluateMonthlySummaryAsync(userId, today));

        return candidates;
    }

    private async Task<IReadOnlyList<NotificationCandidate>> EvaluateRecurringExpensesAsync(int userId, DateTime today)
    {
        var previousMonthReference = today.AddMonths(-1);

        var currentMonthTransactions = (await _transactions.GetByMonthAsync(userId, today.Year, today.Month))
            .Where(t => t.CodTypeTransaction == "E" && t.Status != "C")
            .ToList();

        var previousMonthTransactions = (await _transactions.GetByMonthAsync(userId, previousMonthReference.Year, previousMonthReference.Month))
            .Where(t => t.CodTypeTransaction == "E" && t.Status != "C")
            .ToList();

        var previousByKey = previousMonthTransactions
            .GroupBy(t => (t.CategoryId, Description: t.Description.Trim().ToUpperInvariant()))
            .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));

        var candidates = new List<NotificationCandidate>();

        foreach (var group in currentMonthTransactions.GroupBy(t => (t.CategoryId, Description: t.Description.Trim().ToUpperInvariant())))
        {
            if (!previousByKey.TryGetValue(group.Key, out var previousAmount) || previousAmount <= 0)
                continue;

            var currentAmount = group.Sum(t => t.Amount);
            if (currentAmount <= previousAmount * RecurringIncreaseThreshold)
                continue;

            var representative = group.First();
            var args = new Dictionary<string, string>
            {
                ["description"] = representative.Description,
                ["previousAmount"] = previousAmount.ToString("F2"),
                ["currentAmount"] = currentAmount.ToString("F2")
            };

            candidates.Add(new NotificationCandidate(
                NotificationType.RecurringExpenseIncreased,
                NotificationCriticality.Important,
                MessageKeys.NotificationRecurringExpenseIncreased,
                args, "Transaction", representative.Id));
        }

        return candidates;
    }

    private async Task<IReadOnlyList<NotificationCandidate>> EvaluateNegativeBalanceAsync(int userId, DateTime today)
    {
        var context = await _projectionRunner.RunAsync(userId, today, PlanningDefaults.DefaultHorizonMonths, null);
        var firstNegativeMonth = context.Projection.FirstNegativeMonth;

        if (firstNegativeMonth is null)
            return Array.Empty<NotificationCandidate>();

        var args = new Dictionary<string, string>
        {
            ["month"] = firstNegativeMonth.Month.ToString(),
            ["year"] = firstNegativeMonth.Year.ToString()
        };

        return new[]
        {
            new NotificationCandidate(
                NotificationType.ProjectedNegativeBalance,
                NotificationCriticality.Critical,
                MessageKeys.NotificationProjectedNegativeBalance,
                args, null, null)
        };
    }

    private async Task<IReadOnlyList<NotificationCandidate>> EvaluateCategoryTrendsAsync(int userId, DateTime today)
    {
        var expenseCategories = (await _categories.GetAllAsync(userId))
            .Where(c => c.CodTypeTransaction == "E")
            .ToList();

        if (expenseCategories.Count == 0)
            return Array.Empty<NotificationCandidate>();

        var candidates = new List<NotificationCandidate>();
        var currentMonthStart = new DateTime(today.Year, today.Month, 1);
        var historyStart = currentMonthStart.AddMonths(-TrendHistoryMonths);

        var historyTransactions = (await _transactions.GetByPeriodAsync(userId, historyStart, today))
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

            var historicalAverage = monthlyTotals.Count > 0 ? monthlyTotals.Average() : 0m;
            var currentSpent = categoryTransactions
                .Where(t => t.TransactionDate.Year == today.Year && t.TransactionDate.Month == today.Month)
                .Sum(t => t.Amount);

            if (historicalAverage <= 0 || currentSpent <= 0)
                continue;

            var candidate = BuildCategoryTrendCandidate(category, currentSpent, historicalAverage, monthlyTotals);
            if (candidate is null)
                continue;

            var hasBudgetGoal = await _categoryBudgetGoals.GetByCategoryIdAsync(category.Id, userId) is not null;
            if (!hasBudgetGoal && currentSpent > historicalAverage * HighSpendingWithoutBudgetThreshold)
            {
                var args = new Dictionary<string, string>
                {
                    ["categoryName"] = category.Description,
                    ["amount"] = currentSpent.ToString("F2")
                };

                candidates.Add(new NotificationCandidate(
                    NotificationType.CategoryWithoutBudgetGoalHighSpending,
                    NotificationCriticality.Informative,
                    MessageKeys.NotificationCategoryWithoutBudgetGoalHighSpending,
                    args, "Category", category.Id));

                continue;
            }

            candidates.Add(candidate);
        }

        return candidates;
    }

    private NotificationCandidate? BuildCategoryTrendCandidate(
        Category category, decimal currentSpent, decimal historicalAverage, List<decimal> monthlyTotals)
    {
        var direction = _trendCalculator.ComputeDirection(monthlyTotals);

        if (direction == SpendingTrendDirection.Rising && currentSpent > historicalAverage * SignificantIncreaseThreshold)
        {
            var args = new Dictionary<string, string>
            {
                ["categoryName"] = category.Description,
                ["percent"] = Math.Round(((currentSpent / historicalAverage) - 1) * 100, 0).ToString("F0")
            };

            return new NotificationCandidate(
                NotificationType.SignificantSpendingIncrease,
                NotificationCriticality.Important,
                MessageKeys.NotificationSignificantSpendingIncrease,
                args, "Category", category.Id);
        }

        if (currentSpent > historicalAverage * AboveAverageThreshold)
        {
            var args = new Dictionary<string, string> { ["categoryName"] = category.Description };

            return new NotificationCandidate(
                NotificationType.CategorySpendingAboveAverage,
                NotificationCriticality.Informative,
                MessageKeys.NotificationCategorySpendingAboveAverage,
                args, "Category", category.Id);
        }

        return null;
    }

    private async Task<IReadOnlyList<NotificationCandidate>> EvaluateMonthlySummaryAsync(int userId, DateTime today)
    {
        var previousMonthReference = today.AddMonths(-1);

        var currentMonthTransactions = (await _transactions.GetByMonthAsync(userId, today.Year, today.Month))
            .Where(t => t.Status != "C" && t.TransactionDate.Day <= today.Day)
            .ToList();

        var previousMonthTransactions = (await _transactions.GetByMonthAsync(userId, previousMonthReference.Year, previousMonthReference.Month))
            .Where(t => t.Status != "C" && t.TransactionDate.Day <= today.Day)
            .ToList();

        var currentExpense = currentMonthTransactions.Where(t => t.CodTypeTransaction == "E").Sum(t => t.Amount);
        var previousExpense = previousMonthTransactions.Where(t => t.CodTypeTransaction == "E").Sum(t => t.Amount);
        var currentIncome = currentMonthTransactions.Where(t => t.CodTypeTransaction == "I").Sum(t => t.Amount);
        var previousIncome = previousMonthTransactions.Where(t => t.CodTypeTransaction == "I").Sum(t => t.Amount);

        var candidates = new List<NotificationCandidate>();

        if (previousExpense > 0 && currentExpense < previousExpense * SavingsThreshold)
        {
            var savings = previousExpense - currentExpense;
            var args = new Dictionary<string, string> { ["amount"] = savings.ToString("F2") };

            candidates.Add(new NotificationCandidate(
                NotificationType.MonthlySavingsSummary,
                NotificationCriticality.Informative,
                MessageKeys.NotificationMonthlySavingsSummary,
                args, null, null));
        }

        var currentNet = currentIncome - currentExpense;
        var previousNet = previousIncome - previousExpense;

        if (previousNet != 0)
        {
            var percentChange = (currentNet - previousNet) / Math.Abs(previousNet) * 100;
            if (Math.Abs(percentChange) >= NetWorthChangePercentThreshold)
            {
                var args = new Dictionary<string, string>
                {
                    ["percent"] = Math.Round(percentChange, 0).ToString("F0")
                };

                candidates.Add(new NotificationCandidate(
                    NotificationType.NetWorthChange,
                    NotificationCriticality.Informative,
                    MessageKeys.NotificationNetWorthChange,
                    args, null, null));
            }
        }

        return candidates;
    }
}
