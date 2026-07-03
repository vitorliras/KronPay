using System.Globalization;
using Domain.Enums.Planning;
using Domain.Models.Planning;
using Shared.Localization;

namespace Domain.Services.Planning.Rules;

public sealed class DecliningBalanceRule : IViabilityRule
{
    private const int MaxPenalty = 65;

    public RuleResult Evaluate(FinancialProjection projection, ProjectionParameters parameters)
    {
        if (projection.Months.Count == 0)
            return RuleResult.Ok(nameof(DecliningBalanceRule));

        var totalLoss = parameters.InitialBalance - projection.FinalBalance;
        if (totalLoss <= 0)
            return RuleResult.Ok(nameof(DecliningBalanceRule));

        var income = PlanningMetrics.MonthlyIncome(projection);
        var monthlyLoss = totalLoss / projection.Months.Count;
        var burnRatio = monthlyLoss / income;
        var lossRatio = parameters.InitialBalance > 0 ? totalLoss / parameters.InitialBalance : 1m;

        var severity = Math.Min(1m, Math.Max(burnRatio, lossRatio));
        var penalty = (int)Math.Round(severity * MaxPenalty);

        if (penalty <= 0)
            return RuleResult.Ok(nameof(DecliningBalanceRule));

        var args = new Dictionary<string, string>
        {
            ["amount"] = totalLoss.ToString("0.00", CultureInfo.InvariantCulture),
        };

        return new RuleResult(
            nameof(DecliningBalanceRule),
            RuleStatus.Warning,
            penalty,
            IsVeto: false,
            MessageKeys.PlanningWarningDecliningBalance,
            null,
            null,
            args);
    }
}
