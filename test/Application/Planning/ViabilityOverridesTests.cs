using Application.Planning;
using Domain.Enums.Planning;
using Domain.Models.Planning;
using Shouldly;

namespace Tests.Application.Planning;

public class ViabilityOverridesTests
{
    [Fact]
    public void Veto_de_limite_forca_risco_score_baixo_e_adiciona_achado()
    {
        var original = new ViabilityResult(80, ViabilityVerdict.Recommended, new List<RuleResult>());

        var result = ViabilityOverrides.WithCardLimitVeto(original);

        result.Verdict.ShouldBe(ViabilityVerdict.Risk);
        result.Score.ShouldBeLessThanOrEqualTo(20);
        result.Findings.ShouldContain(f => f.RuleName == "CardLimitRule" && f.IsVeto);
    }
}
