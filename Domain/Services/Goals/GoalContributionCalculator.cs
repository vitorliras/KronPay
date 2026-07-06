using Domain.Entities.Goals;

namespace Domain.Services.Goals;

public sealed class GoalContributionCalculator : IGoalContributionCalculator
{
    public int MonthsRemaining(FinancialGoal goal, DateTime asOf)
    {
        var months = ((goal.TargetDate.Year - asOf.Year) * 12) + (goal.TargetDate.Month - asOf.Month);
        return Math.Max(months, 1);
    }

    public decimal RecommendedMonthlyContribution(FinancialGoal goal, DateTime asOf)
    {
        var remaining = goal.TargetAmount - goal.CurrentAmount;
        if (remaining <= 0)
            return 0;

        return remaining / MonthsRemaining(goal, asOf);
    }
}
