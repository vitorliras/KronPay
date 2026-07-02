using Application.DTOs.Planning;
using Domain.Models.Planning;

namespace Application.Planning;

public static class PlanningResponseMapper
{
    public static IReadOnlyList<ProjectionMonthResponse> MapMonths(FinancialProjection projection)
        => projection.Months
            .Select(m => new ProjectionMonthResponse(
                m.Year, m.Month,
                m.OpeningBalance, m.Inflows,
                m.PredictedOutflow, m.ProbableOutflow,
                m.PredictedClosing, m.ProbableClosing))
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
