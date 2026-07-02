using Domain.Enums.Planning;
using Domain.Models.Planning;
using Domain.Services.Planning;
using Shouldly;

namespace Tests.Domain.Services;

public class FinancialProjectionServiceTests
{
    private readonly FinancialProjectionService _sut = new();

    private static ProjectionParameters Params(decimal initial, int horizon = 3)
        => new(new DateTime(2026, 6, 15), horizon, initial);

    private static FinancialFlow Flow(
        int year, int month, int day,
        FlowDirection direction,
        decimal amount,
        ConfidenceLevel confidence = ConfidenceLevel.High)
        => new(new DateTime(year, month, day), direction, amount, confidence, FlowOrigin.PlannedCommitment);

    [Fact]
    public void Sem_fluxos_mantem_o_saldo_inicial_nos_dois_cenarios()
    {
        var projection = _sut.Project(Array.Empty<FinancialFlow>(), Params(500m));

        projection.Months.Count.ShouldBe(3);
        projection.Months.ShouldAllBe(m => m.PredictedClosing == 500m && m.ProbableClosing == 500m);
        projection.FinalBalance.ShouldBe(500m);
    }

    [Fact]
    public void Saida_comprometida_reduz_previsto_e_provavel_igualmente()
    {
        var flows = new[] { Flow(2026, 6, 20, FlowDirection.Outflow, 100m) };

        var jun = _sut.Project(flows, Params(500m)).Months[0];

        jun.PredictedOutflow.ShouldBe(100m);
        jun.ProbableOutflow.ShouldBe(100m);
        jun.PredictedClosing.ShouldBe(400m);
        jun.ProbableClosing.ShouldBe(400m);
    }

    [Fact]
    public void Saida_estimada_afeta_so_o_provavel()
    {
        var flows = new[] { Flow(2026, 6, 20, FlowDirection.Outflow, 100m, ConfidenceLevel.Medium) };

        var jun = _sut.Project(flows, Params(500m)).Months[0];

        jun.PredictedOutflow.ShouldBe(0m);
        jun.ProbableOutflow.ShouldBe(100m);
        jun.PredictedClosing.ShouldBe(500m);
        jun.ProbableClosing.ShouldBe(400m);
    }

    [Fact]
    public void Combina_entrada_comprometida_com_saida_estimada()
    {
        var flows = new[]
        {
            Flow(2026, 6, 10, FlowDirection.Inflow, 1000m, ConfidenceLevel.High),
            Flow(2026, 6, 20, FlowDirection.Outflow, 300m, ConfidenceLevel.Medium)
        };

        var jun = _sut.Project(flows, Params(0m)).Months[0];

        jun.Inflows.ShouldBe(1000m);
        jun.PredictedClosing.ShouldBe(1000m);
        jun.ProbableClosing.ShouldBe(700m);
    }

    [Fact]
    public void Primeiro_mes_negativo_usa_o_cenario_provavel()
    {
        var flows = new[] { Flow(2026, 7, 10, FlowDirection.Outflow, 100m, ConfidenceLevel.Medium) };

        var projection = _sut.Project(flows, Params(50m));

        projection.FirstNegativeMonth.ShouldNotBeNull();
        projection.FirstNegativeMonth!.Month.ShouldBe(7);
    }
}
