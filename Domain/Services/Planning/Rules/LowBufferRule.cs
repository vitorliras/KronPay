using System.Globalization;
using Domain.Enums.Planning;
using Domain.Models.Planning;
using Shared.Localization;

namespace Domain.Services.Planning.Rules;

public sealed class LowBufferRule : IViabilityRule
{
    private const int MaxPenalty = 25;
    private const decimal SafeMonths = 3m;

    public RuleResult Evaluate(FinancialProjection projection, ProjectionParameters parameters)
    {
        if (projection.Months.Count == 0)
            return RuleResult.Ok(nameof(LowBufferRule));

        var minClosing = projection.Months.Min(m => m.ProbableClosing);
        if (minClosing < 0)
            return RuleResult.Ok(nameof(LowBufferRule));

        var outflow = PlanningMetrics.MonthlyOutflow(projection);
        var bufferMonths = minClosing / outflow;
        if (bufferMonths >= SafeMonths)
            return RuleResult.Ok(nameof(LowBufferRule));

        var severity = 1m - (bufferMonths / SafeMonths);
        var penalty = (int)Math.Round(severity * MaxPenalty);

        if (penalty <= 0)
            return RuleResult.Ok(nameof(LowBufferRule));

        var args = new Dictionary<string, string>
        {
            ["months"] = bufferMonths.ToString("0.0", CultureInfo.InvariantCulture),
        };

        return new RuleResult(
            nameof(LowBufferRule),
            RuleStatus.Warning,
            penalty,
            IsVeto: false,
            MessageKeys.PlanningWarningLowBuffer,
            null,
            null,
            args);
    }
}
