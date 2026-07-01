using Application.Planning.Flows;
using Domain.Entities.Transactions;
using Domain.Enums.Planning;
using Domain.Interfaces.Transactions;
using Moq;
using Shouldly;

namespace Tests.Application.Planning.Flows;

public class TransactionFlowSourceTests
{
    private readonly Mock<ITransactionRepository> _repo = new();
    private readonly TransactionFlowSource _sut;

    public TransactionFlowSourceTests() => _sut = new TransactionFlowSource(_repo.Object);

    [Fact]
    public async Task Mapeia_apenas_transacoes_em_aberto_com_direcao_correta()
    {
        var from = new DateTime(2026, 6, 1);
        var to = new DateTime(2026, 8, 31);

        var transactions = new List<Transaction>
        {
            new(1, 3000m, new DateTime(2026, 6, 5), "Salário", "I", status: "O"),
            new(1, 200m, new DateTime(2026, 6, 10), "Conta de luz", "E", status: "O"),
            new(1, 999m, new DateTime(2026, 6, 12), "Já pago", "E", status: "P")
        };

        _repo.Setup(r => r.GetByPeriodAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(transactions);

        var flows = (await _sut.GetFlowsAsync(1, from, to)).ToList();

        flows.Count.ShouldBe(2);
        flows.ShouldContain(f => f.Direction == FlowDirection.Inflow && f.Amount == 3000m);
        flows.ShouldContain(f => f.Direction == FlowDirection.Outflow && f.Amount == 200m);
        flows.ShouldAllBe(f => f.Confidence == ConfidenceLevel.High && f.Origin == FlowOrigin.Transaction);
    }
}
