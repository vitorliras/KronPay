using Application.Goals;
using Domain.Entities.Goals;
using Domain.Enums.Notifications;
using Domain.Interfaces;
using Domain.Interfaces.Goals;
using Domain.Interfaces.Transactions;
using Shared.Localization;

namespace Application.Notifications.Rules;

public sealed class GoalNotificationRuleEvaluator : IGoalNotificationRuleEvaluator
{
    private const int ContributionOverdueThresholdDays = 60;
    private const int NearCompletionMinDaysSinceContribution = 30;
    private const decimal NearCompletionProgressThreshold = 0.9m;
    private const int MonthlyReminderDayOfMonth = 20;
    private const decimal CategoryBudgetNearLimitThreshold = 0.8m;

    private readonly IFinancialGoalRepository _financialGoals;
    private readonly ICategoryBudgetGoalRepository _categoryBudgetGoals;
    private readonly ITransactionRepository _transactions;
    private readonly ICategoryRepository _categories;
    private readonly IGoalRiskEvaluator _riskEvaluator;

    public GoalNotificationRuleEvaluator(
        IFinancialGoalRepository financialGoals,
        ICategoryBudgetGoalRepository categoryBudgetGoals,
        ITransactionRepository transactions,
        ICategoryRepository categories,
        IGoalRiskEvaluator riskEvaluator)
    {
        _financialGoals = financialGoals;
        _categoryBudgetGoals = categoryBudgetGoals;
        _transactions = transactions;
        _categories = categories;
        _riskEvaluator = riskEvaluator;
    }

    public async Task<IReadOnlyList<NotificationCandidate>> EvaluateAsync(int userId, DateTime today)
    {
        var candidates = new List<NotificationCandidate>();

        candidates.AddRange(await EvaluateFinancialGoalsAsync(userId, today));
        candidates.AddRange(await EvaluateCategoryBudgetGoalsAsync(userId, today));

        return candidates;
    }

    private async Task<IReadOnlyList<NotificationCandidate>> EvaluateFinancialGoalsAsync(int userId, DateTime today)
    {
        var goals = await _financialGoals.GetActiveAsync(userId);
        var candidates = new List<NotificationCandidate>();

        foreach (var goal in goals)
        {
            if (await _riskEvaluator.IsAtRiskAsync(userId, goal, today))
            {
                var riskArgs = new Dictionary<string, string>
                {
                    ["goalName"] = goal.Description,
                    ["targetDate"] = goal.TargetDate.ToString("dd/MM/yyyy")
                };

                candidates.Add(new NotificationCandidate(
                    NotificationType.FinancialGoalAtRisk,
                    NotificationCriticality.Important,
                    MessageKeys.NotificationFinancialGoalAtRisk,
                    riskArgs, "FinancialGoal", goal.Id));
            }

            var contributionCandidate = BuildContributionCandidate(goal, today);
            if (contributionCandidate is not null)
                candidates.Add(contributionCandidate);
        }

        return candidates;
    }

    private static NotificationCandidate? BuildContributionCandidate(FinancialGoal goal, DateTime today)
    {
        var lastContribution = goal.LastContributionAt?.Date ?? goal.CreatedAt.Date;
        var daysSinceLastContribution = (today - lastContribution).Days;
        var progress = goal.TargetAmount > 0 ? goal.CurrentAmount / goal.TargetAmount : 0;

        // Ordem de prioridade: só um candidato de aporte por meta, para não empilhar
        // 3 notificações parecidas sobre a mesma meta ao mesmo tempo.
        if (progress >= NearCompletionProgressThreshold && daysSinceLastContribution >= NearCompletionMinDaysSinceContribution)
        {
            var args = BuildGoalArgs(goal);
            return new NotificationCandidate(
                NotificationType.FinancialGoalNearCompletionReminder,
                NotificationCriticality.Important,
                MessageKeys.NotificationFinancialGoalNearCompletionReminder,
                args, "FinancialGoal", goal.Id);
        }

        if (daysSinceLastContribution >= ContributionOverdueThresholdDays)
        {
            var args = new Dictionary<string, string>
            {
                ["goalName"] = goal.Description,
                ["days"] = daysSinceLastContribution.ToString()
            };

            return new NotificationCandidate(
                NotificationType.FinancialGoalContributionOverdue,
                NotificationCriticality.Important,
                MessageKeys.NotificationFinancialGoalContributionOverdue,
                args, "FinancialGoal", goal.Id);
        }

        var noContributionThisMonth = goal.LastContributionAt is null
            || goal.LastContributionAt.Value.Year != today.Year
            || goal.LastContributionAt.Value.Month != today.Month;

        if (today.Day >= MonthlyReminderDayOfMonth && noContributionThisMonth)
        {
            var args = new Dictionary<string, string> { ["goalName"] = goal.Description };

            return new NotificationCandidate(
                NotificationType.FinancialGoalContributionReminder,
                NotificationCriticality.Important,
                MessageKeys.NotificationFinancialGoalContributionReminder,
                args, "FinancialGoal", goal.Id);
        }

        return null;
    }

    private static Dictionary<string, string> BuildGoalArgs(FinancialGoal goal)
    {
        var remaining = goal.TargetAmount - goal.CurrentAmount;
        return new Dictionary<string, string>
        {
            ["goalName"] = goal.Description,
            ["remainingAmount"] = remaining.ToString("F2")
        };
    }

    private async Task<IReadOnlyList<NotificationCandidate>> EvaluateCategoryBudgetGoalsAsync(int userId, DateTime today)
    {
        var budgetGoals = await _categoryBudgetGoals.GetActiveAsync(userId);
        var candidates = new List<NotificationCandidate>();

        var budgetGoalsList = budgetGoals.ToList();
        if (budgetGoalsList.Count == 0)
            return candidates;

        var monthTransactions = await _transactions.GetByMonthAsync(userId, today.Year, today.Month);

        foreach (var goal in budgetGoalsList)
        {
            var spent = monthTransactions
                .Where(t => t.CategoryId == goal.CategoryId && t.CodTypeTransaction == "E" && t.Status != "C")
                .Sum(t => t.Amount);

            if (goal.MonthlyLimit <= 0)
                continue;

            var percentUsed = spent / goal.MonthlyLimit;
            if (percentUsed < CategoryBudgetNearLimitThreshold)
                continue;

            var category = await _categories.GetByIdAsync(goal.CategoryId, userId);
            var categoryName = category?.Description ?? goal.CategoryId.ToString();

            if (percentUsed >= 1m)
            {
                var args = new Dictionary<string, string>
                {
                    ["categoryName"] = categoryName,
                    ["monthlyLimit"] = goal.MonthlyLimit.ToString("F2")
                };

                candidates.Add(new NotificationCandidate(
                    NotificationType.CategoryBudgetGoalExceeded,
                    NotificationCriticality.Critical,
                    MessageKeys.NotificationCategoryBudgetGoalExceeded,
                    args, "CategoryBudgetGoal", goal.Id));
            }
            else
            {
                var args = new Dictionary<string, string>
                {
                    ["categoryName"] = categoryName,
                    ["percentUsed"] = Math.Round(percentUsed * 100, 0).ToString("F0")
                };

                candidates.Add(new NotificationCandidate(
                    NotificationType.CategoryBudgetGoalNearLimit,
                    NotificationCriticality.Important,
                    MessageKeys.NotificationCategoryBudgetGoalNearLimit,
                    args, "CategoryBudgetGoal", goal.Id));
            }
        }

        return candidates;
    }
}
