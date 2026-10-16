using Application.Abstractions.Common;
using Application.DTOs.Card.CardPurchases;
using Application.UseCases.Card.CardPurchases;
using Domain.Entities.Card;
using Domain.Entities.Transactions;
using Domain.Interfaces;
using Domain.Interfaces.Card;
using Moq;
using Shared.Localization;
using Shouldly;

namespace Tests.Application.Card.CardPurchases;

public class ChangeCardPurchaseAmountUseCaseTests
{
    private readonly Mock<ICardPurchaseRepository> _purchaseRepo = new();
    private readonly Mock<ICardInvoiceRepository> _invoiceRepo = new();
    private readonly Mock<ICreditCardRepository> _creditCardRepo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly ChangeCardPurchaseAmountUseCase _sut;

    public ChangeCardPurchaseAmountUseCaseTests()
    {
        _currentUser.Setup(c => c.UserId).Returns(1);
        _uow.Setup(u => u.CommitAsync(It.IsAny<CancellationToken?>())).ReturnsAsync(true);
        _creditCardRepo.Setup(r => r.GetByIdAsync(10, 1)).ReturnsAsync(CreditCard(10, 5000m));
        _purchaseRepo.Setup(r => r.SumPendingInstallmentsByCardAsync(10, 1)).ReturnsAsync(0m);
        _sut = new ChangeCardPurchaseAmountUseCase(
            _purchaseRepo.Object, _invoiceRepo.Object, _creditCardRepo.Object,
            _uow.Object, _currentUser.Object);
    }

    [Fact]
    public async Task Aplica_novo_valor_a_todas_as_parcelas_e_recalcula_faturas_e_total()
    {
        var purchase = Purchase(8, "academia", 240m, 12);
        var invoices = Enumerable.Range(1, 12)
            .Select(n => Invoice(100 + n, 20m))
            .ToArray();
        var installments = invoices
            .Select((inv, idx) => Installment(200 + idx, purchase, inv, (short)(idx + 1), 20m))
            .ToArray();

        Setup(purchase, installments, invoices);
        _purchaseRepo.Setup(r => r.SumPendingInstallmentsByCardAsync(10, 1)).ReturnsAsync(240m);

        var result = await _sut.ExecuteAsync(new ChangeCardPurchaseAmountRequest(8, 30m));

        result.IsSuccess.ShouldBeTrue();
        purchase.TotalAmount.ShouldBe(360m);
        installments.ShouldAllBe(i => i.Amount == 30m);
        invoices.ShouldAllBe(v => v.TotalAmount == 30m);
    }

    [Fact]
    public async Task Recalcula_apenas_a_parcela_desta_compra_dentro_de_fatura_compartilhada()
    {
        var purchase = Purchase(8, "academia", 20m, 1);
        var invoice = Invoice(101, 20m);
        invoice.AddAmount(50m);
        var installment = Installment(201, purchase, invoice, 1, 20m);

        Setup(purchase, new[] { installment }, new[] { invoice });
        _purchaseRepo.Setup(r => r.SumPendingInstallmentsByCardAsync(10, 1)).ReturnsAsync(20m);

        var result = await _sut.ExecuteAsync(new ChangeCardPurchaseAmountRequest(8, 30m));

        result.IsSuccess.ShouldBeTrue();
        invoice.TotalAmount.ShouldBe(80m);
        installment.Amount.ShouldBe(30m);
    }

    [Fact]
    public async Task Bloqueia_quando_ha_parcela_ja_quitada()
    {
        var purchase = Purchase(8, "academia", 40m, 2);
        var open = Invoice(101, 20m);
        var paidInv = Invoice(102, 20m);
        var settled = Installment(201, purchase, paidInv, 1, 20m, settled: true);
        var pending = Installment(202, purchase, open, 2, 20m);

        Setup(purchase, new[] { settled, pending }, new[] { open, paidInv });

        var result = await _sut.ExecuteAsync(new ChangeCardPurchaseAmountRequest(8, 30m));

        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldBe(MessageKeys.CardPurchaseAmountLocked);
        pending.Amount.ShouldBe(20m);
    }

    [Fact]
    public async Task Bloqueia_quando_ha_fatura_paga()
    {
        var purchase = Purchase(8, "academia", 40m, 2);
        var open = Invoice(101, 20m);
        var paidInv = Invoice(102, 20m, paid: true);
        var i1 = Installment(201, purchase, open, 1, 20m);
        var i2 = Installment(202, purchase, paidInv, 2, 20m);

        Setup(purchase, new[] { i1, i2 }, new[] { open, paidInv });

        var result = await _sut.ExecuteAsync(new ChangeCardPurchaseAmountRequest(8, 30m));

        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldBe(MessageKeys.CardPurchaseAmountLocked);
    }

    [Fact]
    public async Task Bloqueia_quando_o_novo_valor_estoura_o_limite_do_cartao()
    {
        var purchase = Purchase(8, "academia", 240m, 12);
        var invoices = Enumerable.Range(1, 12).Select(n => Invoice(100 + n, 20m)).ToArray();
        var installments = invoices
            .Select((inv, idx) => Installment(200 + idx, purchase, inv, (short)(idx + 1), 20m))
            .ToArray();

        Setup(purchase, installments, invoices);
        _creditCardRepo.Setup(r => r.GetByIdAsync(10, 1)).ReturnsAsync(CreditCard(10, 300m));
        _purchaseRepo.Setup(r => r.SumPendingInstallmentsByCardAsync(10, 1)).ReturnsAsync(240m);

        var result = await _sut.ExecuteAsync(new ChangeCardPurchaseAmountRequest(8, 30m));

        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldBe(MessageKeys.CreditLimitExceeded);
    }

    [Fact]
    public async Task Compra_inexistente_falha()
    {
        _purchaseRepo.Setup(r => r.GetByIdAsync(99, 1)).ReturnsAsync((CardPurchase?)null);

        var result = await _sut.ExecuteAsync(new ChangeCardPurchaseAmountRequest(99, 30m));

        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldBe(MessageKeys.ItemNotFound);
    }

    private void Setup(CardPurchase purchase, CardInstallment[] installments, CardInvoice[] invoices)
    {
        _purchaseRepo.Setup(r => r.GetByIdAsync(purchase.Id, 1)).ReturnsAsync(purchase);
        _purchaseRepo.Setup(r => r.GetInstallmentsByPurchaseAsync(purchase.Id, 1)).ReturnsAsync(installments);
        foreach (var invoice in invoices)
            _invoiceRepo.Setup(r => r.GetByIdAsync(invoice.Id, 1)).ReturnsAsync(invoice);
    }

    private static CreditCard CreditCard(int id, decimal limit)
        => new(id, 1, 1, "Inter", 1, 10, limit);

    private static CardPurchase Purchase(int id, string description, decimal total, short installmentsCount)
    {
        var purchase = new CardPurchase(1, 10, description, total, new DateTime(2026, 7, 1), installmentsCount);
        SetProp(purchase, "Id", id);
        return purchase;
    }

    private static CardInvoice Invoice(int id, decimal amount, bool paid = false)
    {
        var invoice = new CardInvoice(1, 10, 2026, 7, new DateTime(2026, 7, 1), new DateTime(2026, 7, 10));
        invoice.AddAmount(amount);
        if (paid)
            invoice.Pay(new Transaction(1, amount, DateTime.UtcNow, "pgto", "E", status: "P"));
        SetProp(invoice, "Id", id);
        return invoice;
    }

    private static CardInstallment Installment(
        int id, CardPurchase purchase, CardInvoice invoice, short number, decimal amount, bool settled = false)
    {
        var installment = new CardInstallment(1, purchase, invoice, number, amount);
        SetProp(installment, "Id", id);
        SetProp(installment, nameof(CardInstallment.CardInvoiceId), invoice.Id);
        if (settled)
            installment.Settle();
        return installment;
    }

    private static void SetProp(object entity, string property, object value)
        => entity.GetType().GetProperty(property)!.SetValue(entity, value);
}
