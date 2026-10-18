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

    private static int _nextId = 1;

    private void Setup(IReadOnlyList<CardInvoice> invoices, Dictionary<int, decimal>? pendingOverride = null)
    {
        _repo.Setup(r => r.GetByUserAsync(It.IsAny<int>())).ReturnsAsync(invoices);
        _repo.Setup(r => r.GetPendingAmountsByUserAsync(It.IsAny<int>()))
            .ReturnsAsync(pendingOverride ?? invoices.Where(i => !i.IsPaid).ToDictionary(i => i.Id, i => i.TotalAmount));
    }

    private static CardInvoice Invoice(short year, short month, DateTime due, decimal amount, bool paid = false)
    {
        var invoice = new CardInvoice(1, 1, year, month, due.AddDays(-7), due);
        typeof(CardInvoice).GetProperty("Id")!.SetValue(invoice, _nextId++);
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
            Invoice(2026, 7, new DateTime(2026, 7, 5), 800m),
            Invoice(2026, 6, new DateTime(2026, 6, 5), 500m, paid: true)
        };

        Setup(invoices);

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
            Invoice(2026, 5, new DateTime(2026, 5, 20), 300m)
        };

        Setup(invoices);

        var flows = (await _sut.GetFlowsAsync(1, from, to)).ToList();

        flows.ShouldHaveSingleItem();
        flows[0].CompetenceDate.ShouldBe(from);
    }

    [Fact]
    public async Task Fatura_aberta_sem_valor_pendente_nao_projeta_saida()
    {
        var invoices = new List<CardInvoice> { Invoice(2026, 10, new DateTime(2026, 10, 10), 1075.25m) };

        Setup(invoices, new Dictionary<int, decimal>());

        (await _sut.GetFlowsAsync(1, new DateTime(2026, 10, 1), new DateTime(2026, 12, 31))).ShouldBeEmpty();
    }

    [Fact]
    public async Task Fatura_parcialmente_paga_projeta_so_o_valor_pendente()
    {
        var invoice = Invoice(2026, 10, new DateTime(2026, 10, 25), 1095.25m);

        Setup(new List<CardInvoice> { invoice }, new Dictionary<int, decimal> { [invoice.Id] = 20m });

        var flows = (await _sut.GetFlowsAsync(1, new DateTime(2026, 10, 1), new DateTime(2026, 12, 31))).ToList();

        flows.ShouldHaveSingleItem();
        flows[0].Amount.ShouldBe(20m);
    }
}
