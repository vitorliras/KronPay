using System.Globalization;
using Domain.Enums.Planning;
using Domain.Models.Planning;
using Shared.Localization;

namespace Domain.Services.Planning.Rules;

public sealed class SafetyReserveRule : IViabilityRule
{
    private const int MaxPenalty = 15;

    public RuleResult Evaluate(FinancialProjection projection, ProjectionParameters parameters)
    {
        if (parameters.SafetyReserve <= 0)
            return RuleResult.Ok(nameof(SafetyReserveRule));

        var breaching = projection.Months
            .Where(m => m.ProbableClosing >= 0 && m.ProbableClosing < parameters.SafetyReserve)
            .ToList();

        if (breaching.Count == 0)
            return RuleResult.Ok(nameof(SafetyReserveRule));

        var worst = breaching.OrderBy(m => m.ProbableClosing).First();
        var gap = parameters.SafetyReserve - worst.ProbableClosing;
        var depth = Math.Min(1m, gap / parameters.SafetyReserve);
        var breadth = (decimal)breaching.Count / projection.Months.Count;
        var penalty = (int)Math.Round((0.6m + 0.4m * breadth) * depth * MaxPenalty);

        var args = new Dictionary<string, string>
        {
            ["month"] = $"{worst.Month}/{worst.Year}",
            ["amount"] = gap.ToString("0.00", CultureInfo.InvariantCulture),
        };

        return new RuleResult(
            nameof(SafetyReserveRule),
            RuleStatus.Warning,
            penalty,
            IsVeto: false,
            MessageKeys.PlanningWarningSafetyReserve,
            worst.Year,
            worst.Month,
            args);
    }
}
