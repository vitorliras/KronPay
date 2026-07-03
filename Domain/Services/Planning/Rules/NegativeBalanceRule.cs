using System.Globalization;
using Domain.Enums.Planning;
using Domain.Models.Planning;
using Shared.Localization;

namespace Domain.Services.Planning.Rules;

public sealed class NegativeBalanceRule : IViabilityRule
{
    private const int BasePenalty = 60;
    private const int DepthPenalty = 35;

    public RuleResult Evaluate(FinancialProjection projection, ProjectionParameters parameters)
    {
        var negative = projection.FirstNegativeMonth;
        if (negative is null)
            return RuleResult.Ok(nameof(NegativeBalanceRule));

        var deepest = Math.Abs(projection.Months.Min(m => m.ProbableClosing));
        var income = PlanningMetrics.MonthlyIncome(projection);
        var depth = Math.Min(1m, deepest / income);
        var penalty = BasePenalty + (int)Math.Round(depth * DepthPenalty);

        var args = new Dictionary<string, string>
        {
            ["month"] = $"{negative.Month}/{negative.Year}",
            ["amount"] = deepest.ToString("0.00", CultureInfo.InvariantCulture),
        };

        return new RuleResult(
            nameof(NegativeBalanceRule),
            RuleStatus.Critical,
            penalty,
            IsVeto: true,
            MessageKeys.PlanningRiskNegativeBalance,
            negative.Year,
            negative.Month,
            args);
    }
}
