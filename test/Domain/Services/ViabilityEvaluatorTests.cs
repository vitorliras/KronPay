using Domain.Enums.Planning;
using Domain.Models.Planning;
using Domain.Services.Planning;
using Domain.Services.Planning.Rules;
using Shouldly;

namespace Tests.Domain.Services;

public class ViabilityEvaluatorTests
{
    private static ViabilityEvaluator Evaluator()
        => new(new IViabilityRule[]
        {
            new NegativeBalanceRule(),
            new SafetyReserveRule(),
            new ConfidenceRule(),
            new DecliningBalanceRule(),
            new LowBufferRule(),
        });

    private static ProjectionParameters Params(decimal initial, decimal reserve = 0m)
        => new(new DateTime(2026, 6, 1), 12, initial, SafetyReserve: reserve);

    private static FinancialProjection Trajectory(
        decimal initial, decimal final, int months, decimal inflows, decimal outflow)
    {
        var list = new List<ProjectionMonth>(months);
        for (var i = 0; i < months; i++)
        {
            var closing = initial + (final - initial) * (i + 1) / months;
            list.Add(new ProjectionMonth(2026, i + 1, 0m, inflows, outflow, outflow, closing, closing));
        }
        return new FinancialProjection(list);
    }

    [Fact]
    public void Trajetoria_estavel_da_score_maximo_e_recomendado()
    {
        var projection = Trajectory(1000m, 1000m, 12, 500m, 300m);

        var result = Evaluator().Evaluate(projection, Params(1000m));

        result.Score.ShouldBe(100);
        result.Verdict.ShouldBe(ViabilityVerdict.Recommended);
        result.Findings.ShouldBeEmpty();
    }

    [Fact]
    public void Perder_muito_dinheiro_cai_para_atencao_forte()
    {
        // 50k -> 20k em 12 meses, renda 5k, gasto 7,5k, sem negativar
        var projection = Trajectory(50000m, 20000m, 12, 5000m, 7500m);

        var result = Evaluator().Evaluate(projection, Params(50000m));

        result.Score.ShouldBe(58);
        result.Verdict.ShouldBe(ViabilityVerdict.Attention);
    }

    [Fact]
    public void Ficar_negativo_veta_para_risco_com_score_baixo()
    {
        var projection = Trajectory(1000m, -500m, 12, 500m, 800m);

        var result = Evaluator().Evaluate(projection, Params(1000m));

        result.Verdict.ShouldBe(ViabilityVerdict.Risk);
        result.Score.ShouldBeLessThanOrEqualTo(20);
        result.Findings.ShouldContain(f => f.RuleName == nameof(NegativeBalanceRule));
    }
}
