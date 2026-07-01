using Domain.Enums.Planning;
using Domain.Models.Planning;
using Domain.Services.Planning;
using Shouldly;

namespace Tests.Domain.Services;

public class FinancialProjectionServiceTests
{
    private readonly FinancialProjectionService _sut = new();

    private static ProjectionParameters Params(decimal initial, int horizon = 3, decimal spread = 0.15m)
        => new(new DateTime(2026, 6, 15), horizon, initial, SafetyReserve: 0m, EstimateSpreadRate: spread);

    private static FinancialFlow Flow(
        int year, int month, int day,
        FlowDirection direction,
        decimal amount,
        ConfidenceLevel confidence = ConfidenceLevel.High)
        => new(new DateTime(year, month, day), direction, amount, confidence, FlowOrigin.PlannedCommitment);

    [Fact]
    public void Sem_fluxos_mantem_o_saldo_inicial_em_todos_os_meses()
    {
        var projection = _sut.Project(Array.Empty<FinancialFlow>(), Params(500m));

        projection.Months.Count.ShouldBe(3);
        projection.Months.ShouldAllBe(m => m.ClosingBalance == 500m);
        projection.FinalBalance.ShouldBe(500m);
    }

    [Fact]
    public void Saida_reduz_o_saldo_do_mes_e_propaga_para_os_seguintes()
    {
        var flows = new[] { Flow(2026, 6, 20, FlowDirection.Outflow, 100m) };

        var projection = _sut.Project(flows, Params(500m));

        projection.Months[0].ClosingBalance.ShouldBe(400m); // jun
        projection.Months[1].OpeningBalance.ShouldBe(400m); // jul herda
        projection.FinalBalance.ShouldBe(400m);
    }

    [Fact]
    public void Separa_comprometido_de_estimado_no_breakdown()
    {
        var flows = new[]
        {
            Flow(2026, 6, 10, FlowDirection.Inflow, 1000m, ConfidenceLevel.High),
            Flow(2026, 6, 20, FlowDirection.Outflow, 300m, ConfidenceLevel.Medium)
        };

        var jun = _sut.Project(flows, Params(0m)).Months[0];

        jun.CommittedNet.ShouldBe(1000m);
        jun.EstimatedNet.ShouldBe(-300m);
        jun.ClosingBalance.ShouldBe(700m);
    }

    [Fact]
    public void Fluxo_estimado_gera_banda_de_confianca()
    {
        var flows = new[] { Flow(2026, 6, 20, FlowDirection.Outflow, 100m, ConfidenceLevel.Medium) };

        var jun = _sut.Project(flows, Params(500m, spread: 0.15m)).Months[0];

        jun.ClosingBalance.ShouldBe(400m);
        jun.OptimisticClosing.ShouldBe(415m);   // +15% da exposição estimada
        jun.PessimisticClosing.ShouldBe(385m);
    }

    [Fact]
    public void Fluxo_comprometido_nao_gera_banda()
    {
        var flows = new[] { Flow(2026, 6, 20, FlowDirection.Outflow, 100m, ConfidenceLevel.High) };

        var jun = _sut.Project(flows, Params(500m)).Months[0];

        jun.OptimisticClosing.ShouldBe(400m);
        jun.PessimisticClosing.ShouldBe(400m);
        jun.ClosingBalance.ShouldBe(400m);
    }

    [Fact]
    public void Detecta_primeiro_mes_negativo_no_cenario_pessimista()
    {
        var flows = new[] { Flow(2026, 7, 10, FlowDirection.Outflow, 100m) };

        var projection = _sut.Project(flows, Params(50m));

        projection.FirstNegativeMonth.ShouldNotBeNull();
        projection.FirstNegativeMonth!.Month.ShouldBe(7);
    }
}
