using Domain.Enums.Planning;
using Domain.Models.Planning;
using Shared.Localization;

namespace Domain.Services.Planning.Rules;

/// <summary>Veto: o cenário pessimista fica negativo em algum mês.</summary>
public sealed class NegativeBalanceRule : IViabilityRule
{
    public RuleResult Evaluate(FinancialProjection projection, ProjectionParameters parameters)
    {
        var negative = projection.FirstNegativeMonth;

        if (negative is null)
            return RuleResult.Ok(nameof(NegativeBalanceRule));

        return new RuleResult(
            nameof(NegativeBalanceRule),
            RuleStatus.Critical,
            Penalty: 60,
            IsVeto: true,
            MessageKeys.PlanningRiskNegativeBalance,
            negative.Year,
            negative.Month);
    }
}
