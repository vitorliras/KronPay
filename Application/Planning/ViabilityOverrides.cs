using Domain.Enums.Planning;
using Domain.Models.Planning;
using Shared.Localization;

namespace Application.Planning;

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
