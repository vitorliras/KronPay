using Domain.Enums.Planning;
using Domain.Models.Planning;
using Shared.Localization;

namespace Domain.Services.Planning.Rules;

public sealed class SafetyReserveRule : IViabilityRule
{
    public RuleResult Evaluate(FinancialProjection projection, ProjectionParameters parameters)
    {
        if (parameters.SafetyReserve <= 0)
            return RuleResult.Ok(nameof(SafetyReserveRule));

        var below = projection.Months.FirstOrDefault(m =>
            m.ProbableClosing >= 0 && m.ProbableClosing < parameters.SafetyReserve);

        if (below is null)
            return RuleResult.Ok(nameof(SafetyReserveRule));

        return new RuleResult(
            nameof(SafetyReserveRule),
            RuleStatus.Warning,
            Penalty: 25,
            IsVeto: false,
            MessageKeys.PlanningWarningSafetyReserve,
            below.Year,
            below.Month);
    }
}
