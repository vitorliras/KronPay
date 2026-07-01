using Domain.Enums.Planning;

namespace Domain.Models.Planning;

public sealed record ViabilityResult(
    int Score,
    ViabilityVerdict Verdict,
    IReadOnlyList<RuleResult> Findings);
