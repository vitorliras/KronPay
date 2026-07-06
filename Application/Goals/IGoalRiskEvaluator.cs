using Domain.Entities.Goals;

namespace Application.Goals;

public interface IGoalRiskEvaluator
{
    Task<bool> IsAtRiskAsync(int userId, FinancialGoal goal, DateTime now);
}
