using Application.Planning;
using Domain.Entities.Goals;
using Domain.Services.Goals;

namespace Application.Goals;

public sealed class GoalRiskEvaluator : IGoalRiskEvaluator
{
    private readonly IProjectionRunner _runner;
    private readonly IGoalContributionCalculator _calculator;

    public GoalRiskEvaluator(IProjectionRunner runner, IGoalContributionCalculator calculator)
    {
        _runner = runner;
        _calculator = calculator;
    }

    public async Task<bool> IsAtRiskAsync(int userId, FinancialGoal goal, DateTime now)
    {
        var monthsRemaining = _calculator.MonthsRemaining(goal, now);
        var horizon = Math.Clamp(monthsRemaining + 3, PlanningDefaults.DefaultHorizonMonths, PlanningDefaults.MaxHorizonMonths);

        var baseline = await _runner.RunAsync(userId, now, horizon, null);
        var reserve = baseline.Parameters.SafetyReserve;
        var months = baseline.Projection.Months;

        for (var i = 0; i < months.Count; i++)
        {
            if (months[i].ProbableClosing - reserve < 0)
                return true;
        }

        return false;
    }
}
