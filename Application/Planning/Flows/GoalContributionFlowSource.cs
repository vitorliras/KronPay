using Domain.Enums.Planning;
using Domain.Interfaces.Goals;
using Domain.Models.Planning;
using Domain.Services.Goals;

namespace Application.Planning.Flows;

public sealed class GoalContributionFlowSource : IFinancialFlowSource
{
    private readonly IFinancialGoalRepository _goals;
    private readonly IGoalContributionCalculator _calculator;

    public GoalContributionFlowSource(IFinancialGoalRepository goals, IGoalContributionCalculator calculator)
    {
        _goals = goals;
        _calculator = calculator;
    }

    public async Task<IEnumerable<FinancialFlow>> GetFlowsAsync(int userId, DateTime from, DateTime to)
    {
        var activeGoals = await _goals.GetActiveAsync(userId);
        var flows = new List<FinancialFlow>();

        foreach (var goal in activeGoals)
        {
            var monthlyContribution = _calculator.RecommendedMonthlyContribution(goal, from);
            if (monthlyContribution <= 0)
                continue;

            for (var date = new DateTime(from.Year, from.Month, 1); date <= to && date <= goal.TargetDate; date = date.AddMonths(1))
            {
                flows.Add(new FinancialFlow(
                    date,
                    FlowDirection.Outflow,
                    monthlyContribution,
                    ConfidenceLevel.High,
                    FlowOrigin.Goal,
                    goal.Description));
            }
        }

        return flows;
    }
}
