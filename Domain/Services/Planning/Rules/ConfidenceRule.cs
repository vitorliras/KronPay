using System.Globalization;
using Domain.Enums.Planning;
using Domain.Models.Planning;
using Shared.Localization;

namespace Domain.Services.Planning.Rules;

public sealed class ConfidenceRule : IViabilityRule
{
    private const int MaxPenalty = 15;

    public RuleResult Evaluate(FinancialProjection projection, ProjectionParameters parameters)
    {
        var totalProbable = projection.Months.Sum(m => m.ProbableOutflow);
        var totalEstimated = projection.Months.Sum(m => m.ProbableOutflow - m.PredictedOutflow);

        if (totalEstimated <= 0 || totalProbable <= 0)
            return RuleResult.Ok(nameof(ConfidenceRule));

        var share = totalEstimated / totalProbable;
        var penalty = (int)Math.Round(share * MaxPenalty);

        if (penalty <= 0)
            return RuleResult.Ok(nameof(ConfidenceRule));

        var args = new Dictionary<string, string>
        {
            ["percent"] = Math.Round(share * 100).ToString("0", CultureInfo.InvariantCulture),
        };

        return new RuleResult(
            nameof(ConfidenceRule),
            RuleStatus.Warning,
            penalty,
            IsVeto: false,
            MessageKeys.PlanningNoteLowConfidence,
            null,
            null,
            args);
    }
}
