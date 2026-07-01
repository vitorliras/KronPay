using Domain.Enums.Planning;
using Domain.Models.Planning;
using Shared.Localization;

namespace Domain.Services.Planning.Rules;

/// <summary>
/// Penalização: o cenário pessimista, mesmo positivo, cai abaixo da reserva de segurança.
/// Só atua quando há reserva configurada (&gt; 0).
/// </summary>
public sealed class SafetyReserveRule : IViabilityRule
{
    public RuleResult Evaluate(FinancialProjection projection, ProjectionParameters parameters)
    {
        if (parameters.SafetyReserve <= 0)
            return RuleResult.Ok(nameof(SafetyReserveRule));

        var below = projection.Months.FirstOrDefault(m =>
            m.PessimisticClosing >= 0 && m.PessimisticClosing < parameters.SafetyReserve);

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
