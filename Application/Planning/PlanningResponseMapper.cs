using Application.DTOs.Planning;
using Domain.Models.Planning;

namespace Application.Planning;

/// <summary>Mapeia os read models do domínio para os DTOs de resposta (reuso pelos use cases).</summary>
public static class PlanningResponseMapper
{
    public static IReadOnlyList<ProjectionMonthResponse> MapMonths(FinancialProjection projection)
        => projection.Months
            .Select(m => new ProjectionMonthResponse(
                m.Year, m.Month,
                m.OpeningBalance, m.Inflows, m.Outflows, m.ClosingBalance,
                m.CommittedNet, m.EstimatedNet,
                m.OptimisticClosing, m.PessimisticClosing))
            .ToList();

    public static ViabilityResponse Map(ViabilityResult viability)
        => new(
            viability.Score,
            viability.Verdict.ToString(),
            viability.Findings
                .Select(f => new ViabilityFindingResponse(
                    f.RuleName, f.Status.ToString(), f.Penalty, f.IsVeto, f.MessageKey, f.Year, f.Month))
                .ToList());
}
