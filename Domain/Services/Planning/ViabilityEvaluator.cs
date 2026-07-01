using Domain.Enums.Planning;
using Domain.Models.Planning;
using Domain.Services.Planning.Rules;

namespace Domain.Services.Planning;

public sealed class ViabilityEvaluator : IViabilityEvaluator
{
    private const int VetoScoreCap = 20;

    private readonly IEnumerable<IViabilityRule> _rules;

    public ViabilityEvaluator(IEnumerable<IViabilityRule> rules)
    {
        _rules = rules;
    }

    public ViabilityResult Evaluate(FinancialProjection projection, ProjectionParameters parameters)
    {
        var findings = _rules
            .Select(rule => rule.Evaluate(projection, parameters))
            .ToList();

        var score = Math.Clamp(100 - findings.Sum(f => f.Penalty), 0, 100);
        var hasVeto = findings.Any(f => f.IsVeto);

        if (hasVeto)
            score = Math.Min(score, VetoScoreCap);

        var verdict = hasVeto
            ? ViabilityVerdict.Risk
            : score >= 70
                ? ViabilityVerdict.Recommended
                : score >= 40
                    ? ViabilityVerdict.Attention
                    : ViabilityVerdict.Risk;

        var reported = findings
            .Where(f => f.Status != RuleStatus.Ok)
            .ToList();

        return new ViabilityResult(score, verdict, reported);
    }
}
