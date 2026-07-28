using Application.Planning.Flows;
using Domain.Entities.Planning;
using Domain.Entities.Transactions;
using Domain.Enums.Planning;
using Domain.Interfaces.Planning;
using Domain.Interfaces.Transactions;
using Domain.Services.Planning;
using Moq;
using Shouldly;

namespace Tests.Application.Planning.Flows;

public class VariableSpendingFlowSourceTests
{
    private readonly Mock<ITransactionRepository> _transactions = new();
    private readonly Mock<IPlannedCommitmentRepository> _commitments = new();
    private readonly VariableSpendingFlowSource _sut;

    public VariableSpendingFlowSourceTests()
        => _sut = new VariableSpendingFlowSource(_transactions.Object, _commitments.Object, new VariableSpendingEstimator());

    [Fact]
    public async Task Resultado_liquido_pior_que_o_comprometido_gera_ajuste()
    {
        var from = new DateTime(2026, 6, 1);
        var to = new DateTime(2026, 6, 30);

        var past = new List<Transaction>
        {
            new(1, 4000m, new DateTime(2026, 3, 15), "Mercado", "E", status: "P"),
            new(1, 3000m, new DateTime(2026, 3, 20), "Salario", "I", status: "P"),
            new(1, 4000m, new DateTime(2026, 4, 15), "Mercado", "E", status: "P"),
            new(1, 3000m, new DateTime(2026, 4, 20), "Salario", "I", status: "P"),
            new(1, 4000m, new DateTime(2026, 5, 15), "Mercado", "E", status: "P"),
            new(1, 3000m, new DateTime(2026, 5, 20), "Salario", "I", status: "P")
        };

        _transactions
            .Setup(r => r.GetByPeriodAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(past);

        _commitments
            .Setup(r => r.GetByUserAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<PlannedCommitment>
            {
                new(1, "Fixas", 2000m, "O", "M", new DateTime(2025, 1, 1)),
                new(1, "Salario", 5000m, "I", "M", new DateTime(2025, 1, 1))
            });

        var flows = (await _sut.GetFlowsAsync(1, from, to)).ToList();

        flows.Single().Amount.ShouldBe(4000m);
        flows.Single().Confidence.ShouldBe(ConfidenceLevel.Medium);
    }

    [Fact]
    public async Task Resultado_liquido_melhor_que_o_comprometido_nao_gera_ajuste()
    {
        var from = new DateTime(2026, 6, 1);
        var to = new DateTime(2026, 6, 30);

        var past = new List<Transaction>
        {
            new(1, 1000m, new DateTime(2026, 3, 15), "Mercado", "E", status: "P"),
            new(1, 6000m, new DateTime(2026, 3, 20), "Salario", "I", status: "P"),
            new(1, 1000m, new DateTime(2026, 4, 15), "Mercado", "E", status: "P"),
            new(1, 6000m, new DateTime(2026, 4, 20), "Salario", "I", status: "P"),
            new(1, 1000m, new DateTime(2026, 5, 15), "Mercado", "E", status: "P"),
            new(1, 6000m, new DateTime(2026, 5, 20), "Salario", "I", status: "P")
        };

        _transactions
            .Setup(r => r.GetByPeriodAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(past);

        _commitments
            .Setup(r => r.GetByUserAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<PlannedCommitment>
            {
                new(1, "Fixas", 2000m, "O", "M", new DateTime(2025, 1, 1)),
                new(1, "Salario", 5000m, "I", "M", new DateTime(2025, 1, 1))
            });

        var flows = (await _sut.GetFlowsAsync(1, from, to)).ToList();

        flows.ShouldBeEmpty();
    }

    [Fact]
    public async Task Resultado_liquido_positivo_mas_abaixo_do_comprometido_gera_ajuste_parcial_preservando_crescimento()
    {
        var from = new DateTime(2026, 6, 1);
        var to = new DateTime(2026, 6, 30);

        var past = new List<Transaction>
        {
            new(1, 6410m, new DateTime(2026, 3, 15), "Mercado", "E", status: "P"),
            new(1, 7210m, new DateTime(2026, 3, 20), "Salario", "I", status: "P"),
            new(1, 6410m, new DateTime(2026, 4, 15), "Mercado", "E", status: "P"),
            new(1, 7210m, new DateTime(2026, 4, 20), "Salario", "I", status: "P"),
            new(1, 6410m, new DateTime(2026, 5, 15), "Mercado", "E", status: "P"),
            new(1, 7210m, new DateTime(2026, 5, 20), "Salario", "I", status: "P")
        };

        _transactions
            .Setup(r => r.GetByPeriodAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(past);

        _commitments
            .Setup(r => r.GetByUserAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<PlannedCommitment>
            {
                new(1, "Fixas", 2210m, "O", "M", new DateTime(2025, 1, 1)),
                new(1, "Salario", 7500m, "I", "M", new DateTime(2025, 1, 1))
            });

        var flows = (await _sut.GetFlowsAsync(1, from, to)).ToList();

        var committedNet = 7500m - 2210m;
        var realNet = 7210m - 6410m;
        flows.Single().Amount.ShouldBe(committedNet - realNet);
        flows.Single().Amount.ShouldBeLessThan(committedNet);
    }

    [Fact]
    public async Task Ajuste_diminui_quanto_mais_longe_no_horizonte()
    {
        var from = new DateTime(2026, 6, 1);
        var to = new DateTime(2026, 11, 30);

        var past = new List<Transaction>
        {
            new(1, 4000m, new DateTime(2026, 3, 15), "Mercado", "E", status: "P"),
            new(1, 3000m, new DateTime(2026, 3, 20), "Salario", "I", status: "P"),
            new(1, 4000m, new DateTime(2026, 4, 15), "Mercado", "E", status: "P"),
            new(1, 3000m, new DateTime(2026, 4, 20), "Salario", "I", status: "P"),
            new(1, 4000m, new DateTime(2026, 5, 15), "Mercado", "E", status: "P"),
            new(1, 3000m, new DateTime(2026, 5, 20), "Salario", "I", status: "P")
        };

        _transactions
            .Setup(r => r.GetByPeriodAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(past);

        _commitments
            .Setup(r => r.GetByUserAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<PlannedCommitment>
            {
                new(1, "Fixas", 2000m, "O", "M", new DateTime(2025, 1, 1)),
                new(1, "Salario", 5000m, "I", "M", new DateTime(2025, 1, 1))
            });

        var flows = (await _sut.GetFlowsAsync(1, from, to)).ToList();

        flows.Count.ShouldBe(6);
        flows[0].Amount.ShouldBe(4000m);
        flows[3].Amount.ShouldBe(2000m);
    }

    [Fact]
    public async Task Resultado_historico_instavel_reduz_o_ajuste_pela_metade()
    {
        var from = new DateTime(2026, 6, 1);
        var to = new DateTime(2026, 6, 30);

        var past = new List<Transaction>
        {
            new(1, 15000m, new DateTime(2026, 1, 15), "Contas", "E", status: "P"),
            new(1, 25000m, new DateTime(2026, 1, 20), "Salario", "I", status: "P"),
            new(1, 15000m, new DateTime(2026, 2, 15), "Contas", "E", status: "P"),
            new(1, 5000m, new DateTime(2026, 2, 20), "Salario", "I", status: "P"),
            new(1, 15000m, new DateTime(2026, 3, 15), "Contas", "E", status: "P"),
            new(1, 15100m, new DateTime(2026, 3, 20), "Salario", "I", status: "P"),
            new(1, 15000m, new DateTime(2026, 4, 15), "Contas", "E", status: "P"),
            new(1, 6000m, new DateTime(2026, 4, 20), "Salario", "I", status: "P"),
            new(1, 15000m, new DateTime(2026, 5, 15), "Contas", "E", status: "P"),
            new(1, 24500m, new DateTime(2026, 5, 20), "Salario", "I", status: "P")
        };

        _transactions
            .Setup(r => r.GetByPeriodAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(past);

        _commitments
            .Setup(r => r.GetByUserAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<PlannedCommitment>
            {
                new(1, "Fixas", 2000m, "O", "M", new DateTime(2025, 1, 1)),
                new(1, "Salario", 5000m, "I", "M", new DateTime(2025, 1, 1))
            });

        var flows = (await _sut.GetFlowsAsync(1, from, to)).ToList();

        flows.Single().Confidence.ShouldBe(ConfidenceLevel.Low);
        flows.Single().Amount.ShouldBe(1450m);
    }

    [Fact]
    public async Task Sem_historico_nenhum_nao_gera_fluxo()
    {
        var from = new DateTime(2026, 6, 1);
        var to = new DateTime(2026, 6, 30);

        _transactions
            .Setup(r => r.GetByPeriodAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Transaction>());

        _commitments
            .Setup(r => r.GetByUserAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<PlannedCommitment>());

        var flows = (await _sut.GetFlowsAsync(1, from, to)).ToList();

        flows.ShouldBeEmpty();
    }
}
