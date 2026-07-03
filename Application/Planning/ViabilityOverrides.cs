using Domain.Enums.Planning;
using Domain.Models.Planning;
using Shared.Localization;

namespace Application.Planning;

public static class ViabilityOverrides
{
    private const int CardLimitPenalty = 60;

    public static ViabilityResult WithCardLimitVeto(ViabilityResult original)
    {
        var findings = original.Findings.ToList();
        findings.Add(new RuleResult(
            "CardLimitRule",
            RuleStatus.Critical,
            CardLimitPenalty,
            IsVeto: true,
            MessageKeys.CreditLimitExceeded));

        var score = Math.Clamp(original.Score - CardLimitPenalty, 0, 100);

        return new ViabilityResult(score, ViabilityVerdict.Risk, findings);
    }
}
