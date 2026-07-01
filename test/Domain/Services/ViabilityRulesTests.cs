using Domain.Enums.Planning;
using Domain.Models.Planning;
using Domain.Services.Planning.Rules;
using Shouldly;

namespace Tests.Domain.Services;

public class ViabilityRulesTests
{
    private static ProjectionParameters Params(decimal reserve = 0m)
        => new(new DateTime(2026, 6, 1), 1, 1000m, SafetyReserve: reserve);

    private static ProjectionMonth Month(int month, decimal closing, decimal pessimistic)
        => new(2026, month, 0m, 0m, 0m, closing, 0m, 0m, closing, pessimistic);

    private static FinancialProjection Projection(params ProjectionMonth[] months)
        => new(months);

    // ---- NegativeBalanceRule ----
    [Fact]
    public void SaldoNegativo_veta_quando_pessimista_fica_negativo()
    {
        var result = new NegativeBalanceRule().Evaluate(
            Projection(Month(6, 100m, -50m)), Params());

        result.IsVeto.ShouldBeTrue();
        result.Status.ShouldBe(RuleStatus.Critical);
        result.Month.ShouldBe(6);
    }

    [Fact]
    public void SaldoNegativo_ok_quando_tudo_positivo()
    {
        var result = new NegativeBalanceRule().Evaluate(
            Projection(Month(6, 100m, 80m)), Params());

        result.Status.ShouldBe(RuleStatus.Ok);
        result.IsVeto.ShouldBeFalse();
    }

    // ---- SafetyReserveRule ----
    [Fact]
    public void Reserva_avisa_quando_pessimista_fica_abaixo_da_reserva()
    {
        var result = new SafetyReserveRule().Evaluate(
            Projection(Month(6, 200m, 50m)), Params(reserve: 100m));

        result.Status.ShouldBe(RuleStatus.Warning);
        result.Penalty.ShouldBe(25);
        result.IsVeto.ShouldBeFalse();
    }

    [Fact]
    public void Reserva_ignorada_quando_nao_configurada()
    {
        var result = new SafetyReserveRule().Evaluate(
            Projection(Month(6, 10m, 5m)), Params(reserve: 0m));

        result.Status.ShouldBe(RuleStatus.Ok);
    }

    // ---- ConfidenceRule ----
    [Fact]
    public void Confianca_penaliza_quando_ha_banda_no_fim_do_horizonte()
    {
        var result = new ConfidenceRule().Evaluate(
            Projection(Month(6, 100m, 80m)), Params());

        result.Status.ShouldBe(RuleStatus.Warning);
        result.Penalty.ShouldBe(10);
        result.IsVeto.ShouldBeFalse();
    }

    [Fact]
    public void Confianca_ok_quando_nao_ha_banda()
    {
        var result = new ConfidenceRule().Evaluate(
            Projection(Month(6, 100m, 100m)), Params());

        result.Status.ShouldBe(RuleStatus.Ok);
    }
}
