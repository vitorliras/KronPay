using Domain.Entities.Goals;

namespace Domain.Services.Goals;

public interface IGoalContributionCalculator
{
    int MonthsRemaining(FinancialGoal goal, DateTime asOf);
    decimal RecommendedMonthlyContribution(FinancialGoal goal, DateTime asOf);
}
