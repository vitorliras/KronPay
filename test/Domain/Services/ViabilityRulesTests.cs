using Domain.Enums.Planning;
using Domain.Models.Planning;
using Domain.Services.Planning.Rules;
using Shouldly;

namespace Tests.Domain.Services;

public class ViabilityRulesTests
{
    private static ProjectionParameters Params(decimal reserve = 0m, decimal initial = 1000m)
        => new(new DateTime(2026, 6, 1), 1, initial, SafetyReserve: reserve);

    private static ProjectionMonth Month(
        int month,
        decimal probableClosing,
        decimal inflows = 0m,
        decimal probableOutflow = 0m,
        decimal predictedOutflow = 0m)
        => new(2026, month, 0m, inflows, predictedOutflow, probableOutflow, 0m, probableClosing);

    private static FinancialProjection Projection(params ProjectionMonth[] months)
        => new(months);

    [Fact]
    public void SaldoNegativo_veta_e_escala_pela_renda()
    {
        var result = new NegativeBalanceRule().Evaluate(
            Projection(Month(6, -50m, inflows: 100m)), Params());

        result.IsVeto.ShouldBeTrue();
        result.Status.ShouldBe(RuleStatus.Critical);
        result.Penalty.ShouldBe(78);
        result.Month.ShouldBe(6);
        result.Args!.ShouldContainKey("amount");
    }

    [Fact]
    public void SaldoNegativo_ok_quando_tudo_positivo()
    {
        var result = new NegativeBalanceRule().Evaluate(
            Projection(Month(6, 80m, inflows: 100m)), Params());

        result.Status.ShouldBe(RuleStatus.Ok);
    }

    [Fact]
    public void Reserva_penaliza_com_peso_reduzido()
    {
        var result = new SafetyReserveRule().Evaluate(
            Projection(Month(6, 50m)), Params(reserve: 100m));

        result.Status.ShouldBe(RuleStatus.Warning);
        result.Penalty.ShouldBe(8);
    }

    [Fact]
    public void Confianca_penaliza_proporcional_ao_percentual_estimado()
    {
        var result = new ConfidenceRule().Evaluate(
            Projection(Month(6, 100m, probableOutflow: 50m, predictedOutflow: 0m)), Params());

        result.Status.ShouldBe(RuleStatus.Warning);
        result.Penalty.ShouldBe(15);
        result.Args!["percent"].ShouldBe("100");
    }

    [Fact]
    public void Tendencia_penaliza_forte_quando_queima_dinheiro()
    {
        var result = new DecliningBalanceRule().Evaluate(
            Projection(Month(6, 400m, inflows: 500m)), Params(initial: 1000m));

        result.Status.ShouldBe(RuleStatus.Warning);
        result.Penalty.ShouldBe(65);
        result.IsVeto.ShouldBeFalse();
        result.Args!.ShouldContainKey("amount");
    }

    [Fact]
    public void Tendencia_ok_quando_saldo_sobe()
    {
        var result = new DecliningBalanceRule().Evaluate(
            Projection(Month(6, 1200m, inflows: 500m)), Params(initial: 1000m));

        result.Status.ShouldBe(RuleStatus.Ok);
    }

    [Fact]
    public void Colchao_penaliza_quando_abaixo_de_tres_meses()
    {
        var result = new LowBufferRule().Evaluate(
            Projection(Month(6, 300m, probableOutflow: 200m)), Params());

        result.Status.ShouldBe(RuleStatus.Warning);
        result.Penalty.ShouldBe(12);
        result.Args!["months"].ShouldBe("1.5");
    }

    [Fact]
    public void Colchao_ok_quando_tem_tres_meses_ou_mais()
    {
        var result = new LowBufferRule().Evaluate(
            Projection(Month(6, 900m, probableOutflow: 200m)), Params());

        result.Status.ShouldBe(RuleStatus.Ok);
    }

    [Fact]
    public void Colchao_ignorado_quando_saldo_negativo()
    {
        var result = new LowBufferRule().Evaluate(
            Projection(Month(6, -50m, probableOutflow: 200m)), Params());

        result.Status.ShouldBe(RuleStatus.Ok);
    }
}
