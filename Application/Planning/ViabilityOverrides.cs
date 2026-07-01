using Domain.Enums.Planning;
using Domain.Models.Planning;
using Shared.Localization;

namespace Application.Planning;

/// <summary>
/// Aplica o veto de <b>limite do cartão</b> à viabilidade. Esse critério não vem da projeção
/// (depende do limite × uso do cartão), então é aplicado no use case como override.
/// </summary>
public static class ViabilityOverrides
{
    private const int VetoScoreCap = 20;

    public static ViabilityResult WithCardLimitVeto(ViabilityResult original)
    {
        var findings = original.Findings.ToList();
        findings.Add(new RuleResult(
            "CardLimitRule",
            RuleStatus.Critical,
            Penalty: 60,
            IsVeto: true,
            MessageKeys.CreditLimitExceeded));

        return new ViabilityResult(
            Math.Min(original.Score, VetoScoreCap),
            ViabilityVerdict.Risk,
            findings);
    }
}
