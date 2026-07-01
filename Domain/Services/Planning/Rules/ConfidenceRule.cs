using Domain.Enums.Planning;
using Domain.Models.Planning;
using Shared.Localization;

namespace Domain.Services.Planning.Rules;

/// <summary>
/// Penalização leve (nunca veto): há incerteza estimada relevante na projeção — quando o
/// cenário pessimista diverge do esperado (banda não trivial no fim do horizonte).
/// </summary>
public sealed class ConfidenceRule : IViabilityRule
{
    public RuleResult Evaluate(FinancialProjection projection, ProjectionParameters parameters)
    {
        if (projection.Months.Count == 0)
            return RuleResult.Ok(nameof(ConfidenceRule));

        var last = projection.Months[^1];
        var band = last.ClosingBalance - last.PessimisticClosing;

        if (band <= 0)
            return RuleResult.Ok(nameof(ConfidenceRule));

        return new RuleResult(
            nameof(ConfidenceRule),
            RuleStatus.Warning,
            Penalty: 10,
            IsVeto: false,
            MessageKeys.PlanningNoteLowConfidence);
    }
}
