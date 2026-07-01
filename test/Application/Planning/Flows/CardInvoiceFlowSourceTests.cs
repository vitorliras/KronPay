using Application.Planning.Flows;
using Domain.Entities.Card;
using Domain.Entities.Transactions;
using Domain.Enums.Planning;
using Domain.Interfaces.Card;
using Moq;
using Shouldly;

namespace Tests.Application.Planning.Flows;

public class CardInvoiceFlowSourceTests
{
    private readonly Mock<ICardInvoiceRepository> _repo = new();
    private readonly CardInvoiceFlowSource _sut;

    public CardInvoiceFlowSourceTests() => _sut = new CardInvoiceFlowSource(_repo.Object);

    private static CardInvoice Invoice(short year, short month, DateTime due, decimal amount, bool paid = false)
    {
        var invoice = new CardInvoice(1, 1, year, month, due.AddDays(-7), due);
        if (amount > 0) invoice.AddAmount(amount);
        if (paid) invoice.Pay(new Transaction(1, amount, DateTime.UtcNow, "pgto", "E", status: "P"));
        return invoice;
    }

    [Fact]
    public async Task Mapeia_faturas_nao_pagas_como_saida_e_ignora_as_pagas()
    {
        var from = new DateTime(2026, 6, 1);
        var to = new DateTime(2026, 8, 31);

        var invoices = new List<CardInvoice>
        {
            Invoice(2026, 7, new DateTime(2026, 7, 5), 800m),                 // aberta
            Invoice(2026, 6, new DateTime(2026, 6, 5), 500m, paid: true)      // paga -> ignorada
        };

        _repo.Setup(r => r.GetByUserAsync(It.IsAny<int>())).ReturnsAsync(invoices);

        var flows = (await _sut.GetFlowsAsync(1, from, to)).ToList();

        flows.ShouldHaveSingleItem();
        flows[0].Direction.ShouldBe(FlowDirection.Outflow);
        flows[0].Amount.ShouldBe(800m);
        flows[0].CompetenceDate.ShouldBe(new DateTime(2026, 7, 5));
        flows[0].Confidence.ShouldBe(ConfidenceLevel.High);
    }

    [Fact]
    public async Task Fatura_vencida_e_em_aberto_e_ancorada_no_inicio_da_janela()
    {
        var from = new DateTime(2026, 6, 1);
        var to = new DateTime(2026, 8, 31);

        var invoices = new List<CardInvoice>
        {
            Invoice(2026, 5, new DateTime(2026, 5, 20), 300m) // venceu antes da janela, ainda aberta
        };

        _repo.Setup(r => r.GetByUserAsync(It.IsAny<int>())).ReturnsAsync(invoices);

        var flows = (await _sut.GetFlowsAsync(1, from, to)).ToList();

        flows.ShouldHaveSingleItem();
        flows[0].CompetenceDate.ShouldBe(from); // clamp para o início da janela
    }
}
