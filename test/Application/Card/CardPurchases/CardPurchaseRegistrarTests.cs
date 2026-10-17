using DomainEntities = global::Domain.Entities;
using Application.Services.Card;
using Domain.Entities.Card;
using Domain.Interfaces.Card;
using Domain.Services.Card;
using Moq;
using Shouldly;

namespace Tests.Application.Card.CardPurchases;

public class CardPurchaseRegistrarTests
{
    private readonly Mock<ICardPurchaseRepository> _purchases = new();
    private readonly Mock<ICardInvoiceRepository> _invoices = new();
    private readonly Mock<ICreditCardBillingCalculator> _billing = new();
    private readonly List<CardInvoice> _createdInvoices = new();
    private readonly List<CardInstallment> _installments = new();

    public CardPurchaseRegistrarTests()
    {
        _purchases.Setup(p => p.AddAsync(It.IsAny<CardPurchase>())).ReturnsAsync(true);
        _purchases
            .Setup(p => p.AddInstallmentsAsync(It.IsAny<IEnumerable<CardInstallment>>()))
            .Callback<IEnumerable<CardInstallment>>(i => _installments.AddRange(i))
            .ReturnsAsync(true);

        _invoices
            .Setup(i => i.GetByCycleAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<short>(), It.IsAny<short>()))
            .ReturnsAsync((CardInvoice?)null);
        _invoices
            .Setup(i => i.AddAsync(It.IsAny<CardInvoice>()))
            .Callback<CardInvoice>(_createdInvoices.Add)
            .ReturnsAsync(true);

        _billing
            .Setup(b => b.Resolve(It.IsAny<CreditCard>(), It.IsAny<DateTime>()))
            .Returns<CreditCard, DateTime>((_, date) => new BillingCycle(
                (short)date.Year,
                (short)date.Month,
                new DateTime(date.Year, date.Month, 20),
                new DateTime(date.Year, date.Month, 28)));
    }

    private CardPurchaseRegistrationSession Session() =>
        new CardPurchaseRegistrar(_purchases.Object, _invoices.Object, _billing.Object)
            .StartSession(new CreditCard(10, 1, 1, "cartao", 28, 20, 5000m), 1);

    [Fact]
    public async Task Compra_parcelada_gera_uma_parcela_por_fatura_e_soma_o_total()
    {
        var purchase = await Session().RegisterAsync("tenis", 300m, new DateTime(2026, 10, 5), 3, null, null);

        purchase.ShouldNotBeNull();
        _installments.Count.ShouldBe(3);
        _installments.Sum(i => i.Amount).ShouldBe(300m);
        _createdInvoices.Select(i => i.ReferenceMonth).ShouldBe(new short[] { 10, 11, 12 });
    }

    [Fact]
    public async Task Parcelas_com_centavos_fecham_o_total_na_ultima()
    {
        await Session().RegisterAsync("fone", 100m, new DateTime(2026, 10, 5), 3, null, null);

        _installments.Select(i => i.Amount).ShouldBe(new[] { 33.33m, 33.33m, 33.34m });
    }

    [Fact]
    public async Task Duas_compras_no_mesmo_ciclo_criam_uma_unica_fatura()
    {
        var session = Session();

        await session.RegisterAsync("ifood", 45m, new DateTime(2026, 10, 5), 1, null, null);
        await session.RegisterAsync("uber", 23m, new DateTime(2026, 10, 9), 1, null, null);

        _createdInvoices.Count.ShouldBe(1);
        _createdInvoices.Single().TotalAmount.ShouldBe(68m);
        _installments.Count.ShouldBe(2);
        _installments.Select(i => i.CardInvoice).Distinct().Count().ShouldBe(1);
    }

    [Fact]
    public async Task Fatura_existente_no_banco_e_reaproveitada_sem_criar_outra()
    {
        var existing = new CardInvoice(1, 10, 2026, 10, new DateTime(2026, 10, 20), new DateTime(2026, 10, 28));
        _invoices
            .Setup(i => i.GetByCycleAsync(10, 1, 2026, 10))
            .ReturnsAsync(existing);

        await Session().RegisterAsync("ifood", 45m, new DateTime(2026, 10, 5), 1, null, null);

        _createdInvoices.ShouldBeEmpty();
        existing.TotalAmount.ShouldBe(45m);
    }

    [Fact]
    public async Task Fatura_paga_e_reaberta_uma_unica_vez_no_lote()
    {
        var paid = new CardInvoice(1, 10, 2026, 10, new DateTime(2026, 10, 20), new DateTime(2026, 10, 28));
        paid.AddAmount(100m);
        paid.Pay(new DomainEntities.Transactions.Transaction(
            userId: 1, amount: 100m, transactionDate: DateTime.Today, description: "fatura",
            codTypeTransaction: "E", categoryId: null, categoryItemId: null, idPaymentMethod: 1));
        paid.IsPaid.ShouldBeTrue();

        _invoices.Setup(i => i.GetByCycleAsync(10, 1, 2026, 10)).ReturnsAsync(paid);

        var session = Session();
        await session.RegisterAsync("a", 10m, new DateTime(2026, 10, 5), 1, null, null);
        await session.RegisterAsync("b", 20m, new DateTime(2026, 10, 6), 1, null, null);

        paid.IsPaid.ShouldBeFalse();
        paid.TotalAmount.ShouldBe(130m);
    }

    [Fact]
    public async Task Falha_ao_inserir_compra_retorna_nulo()
    {
        _purchases.Setup(p => p.AddAsync(It.IsAny<CardPurchase>())).ReturnsAsync(false);

        var purchase = await Session().RegisterAsync("x", 10m, new DateTime(2026, 10, 5), 1, null, null);

        purchase.ShouldBeNull();
    }

    [Fact]
    public async Task Falha_ao_inserir_fatura_retorna_nulo()
    {
        _invoices.Setup(i => i.AddAsync(It.IsAny<CardInvoice>())).ReturnsAsync(false);

        var purchase = await Session().RegisterAsync("x", 10m, new DateTime(2026, 10, 5), 1, null, null);

        purchase.ShouldBeNull();
    }
}
