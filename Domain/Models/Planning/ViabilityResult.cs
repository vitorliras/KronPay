using Domain.Enums.Planning;

namespace Domain.Models.Planning;

/// <summary>Resultado de uma regra individual (contribui para o score e traz uma mensagem).</summary>
public sealed record RuleResult(
    string RuleName,
    RuleStatus Status,
    int Penalty,
    bool IsVeto,
    string MessageKey,
    int? Year = null,
    int? Month = null)
{
    public static RuleResult Ok(string ruleName)
        => new(ruleName, RuleStatus.Ok, 0, false, string.Empty);
}

/// <summary>Veredito agregado + score (0–100) + os achados que o justificam (transparência).</summary>
public sealed record ViabilityResult(
    int Score,
    ViabilityVerdict Verdict,
    IReadOnlyList<RuleResult> Findings);
