using Domain.Enums.Planning;
using Domain.Models.Planning;
using Shared.Localization;

namespace Domain.Services.Planning.Rules;

public sealed class ConfidenceRule : IViabilityRule
{
    public RuleResult Evaluate(FinancialProjection projection, ProjectionParameters parameters)
    {
        var hasEstimate = projection.Months.Any(m => m.ProbableOutflow > m.PredictedOutflow);

        if (!hasEstimate)
            return RuleResult.Ok(nameof(ConfidenceRule));

        return new RuleResult(
            nameof(ConfidenceRule),
            RuleStatus.Warning,
            Penalty: 10,
            IsVeto: false,
            MessageKeys.PlanningNoteLowConfidence);
    }
}
