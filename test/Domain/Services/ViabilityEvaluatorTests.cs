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
            new ConfidenceRule()
        });

    private static ProjectionParameters Params(decimal reserve = 0m)
        => new(new DateTime(2026, 6, 1), 1, 1000m, SafetyReserve: reserve);

    private static ProjectionMonth Month(decimal closing, decimal pessimistic)
        => new(2026, 6, 0m, 0m, 0m, closing, 0m, 0m, closing, pessimistic);

    [Fact]
    public void Projecao_saudavel_da_score_maximo_e_recomendado()
    {
        var result = Evaluator().Evaluate(
            new FinancialProjection(new[] { Month(1000m, 1000m) }), Params());

        result.Score.ShouldBe(100);
        result.Verdict.ShouldBe(ViabilityVerdict.Recommended);
        result.Findings.ShouldBeEmpty();
    }

    [Fact]
    public void Saldo_negativo_veta_para_risco_com_score_baixo()
    {
        var result = Evaluator().Evaluate(
            new FinancialProjection(new[] { Month(100m, -50m) }), Params());

        result.Verdict.ShouldBe(ViabilityVerdict.Risk);
        result.Score.ShouldBeLessThanOrEqualTo(20);
        result.Findings.ShouldContain(f => f.RuleName == nameof(NegativeBalanceRule));
    }

    [Fact]
    public void Abaixo_da_reserva_com_incerteza_cai_para_atencao()
    {
        // reserva 1500 (pess 900 < reserva) => -25 ; banda (1000-900) => -10 ; score 65 => Atenção
        var result = Evaluator().Evaluate(
            new FinancialProjection(new[] { Month(1000m, 900m) }), Params(reserve: 1500m));

        result.Score.ShouldBe(65);
        result.Verdict.ShouldBe(ViabilityVerdict.Attention);
        result.Findings.Count.ShouldBe(2);
    }
}
