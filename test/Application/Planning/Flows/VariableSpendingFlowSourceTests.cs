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
    public async Task Estima_o_variavel_descontando_os_fixos_do_historico()
    {
        var from = new DateTime(2026, 6, 1);
        var to = new DateTime(2026, 8, 31);

        var past = new List<Transaction>
        {
            new(1, 2800m, new DateTime(2026, 3, 15), "Mercado", "E", status: "P"),
            new(1, 2500m, new DateTime(2026, 4, 15), "Mercado", "E", status: "P"),
            new(1, 3000m, new DateTime(2026, 5, 15), "Mercado", "E", status: "P")
        };

        _transactions
            .Setup(r => r.GetByPeriodAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(past);

        _commitments
            .Setup(r => r.GetByUserAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<PlannedCommitment>
            {
                new(1, "Aluguel", 2000m, "O", "M", new DateTime(2026, 1, 1))
            });

        var flows = (await _sut.GetFlowsAsync(1, from, to)).ToList();

        flows.Count.ShouldBe(3);
        flows.ShouldAllBe(f =>
            f.Direction == FlowDirection.Outflow &&
            f.Amount == 766.67m &&
            f.Confidence == ConfidenceLevel.Medium &&
            f.Origin == FlowOrigin.VariableEstimate);
    }
}
