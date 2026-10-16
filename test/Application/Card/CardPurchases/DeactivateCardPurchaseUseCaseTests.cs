using Application.Abstractions.Common;
using Application.DTOs.Card.CardPurchases;
using Application.UseCases.Card.CardPurchases;
using Domain.Entities.Card;
using Domain.Entities.Transactions;
using Domain.Interfaces;
using Domain.Interfaces.Card;
using Moq;
using Shouldly;

namespace Tests.Application.Card.CardPurchases;

public class DeactivateCardPurchaseUseCaseTests
{
    private readonly Mock<ICardPurchaseRepository> _purchaseRepo = new();
    private readonly Mock<ICardInvoiceRepository> _invoiceRepo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly DeactivateCardPurchaseUseCase _sut;

    public DeactivateCardPurchaseUseCaseTests()
    {
        _currentUser.Setup(c => c.UserId).Returns(1);
        _uow.Setup(u => u.CommitAsync(It.IsAny<CancellationToken?>())).ReturnsAsync(true);
        _sut = new DeactivateCardPurchaseUseCase(
            _purchaseRepo.Object, _invoiceRepo.Object, _uow.Object, _currentUser.Object);
    }

    [Fact]
    public async Task Cancela_apenas_as_parcelas_em_faturas_abertas_e_desativa_a_compra()
    {
        var purchase = Purchase(8, "academia", 499.50m, 5);

        var paidJul = Invoice(15, 2026, 7, 99.90m, paid: true);
        var paidAug = Invoice(16, 2026, 8, 99.90m, paid: true);
        var openSep = Invoice(17, 2026, 9, 99.90m);
        var openOct = Invoice(18, 2026, 10, 99.90m);
        var openNov = Invoice(19, 2026, 11, 99.90m);

        var settledJul = Installment(23, purchase, paidJul, 1, 99.90m, settled: true);
        var settledAug = Installment(24, purchase, paidAug, 2, 99.90m, settled: true);
        var pendingSep = Installment(25, purchase, openSep, 3, 99.90m);
        var pendingOct = Installment(26, purchase, openOct, 4, 99.90m);
        var pendingNov = Installment(27, purchase, openNov, 5, 99.90m);

        var installments = new[] { settledJul, settledAug, pendingSep, pendingOct, pendingNov };
        var invoices = new[] { paidJul, paidAug, openSep, openOct, openNov };

        _purchaseRepo.Setup(r => r.GetByIdAsync(8, 1)).ReturnsAsync(purchase);
        _purchaseRepo.Setup(r => r.GetInstallmentsByPurchaseAsync(8, 1)).ReturnsAsync(installments);
        foreach (var invoice in invoices)
            _invoiceRepo.Setup(r => r.GetByIdAsync(invoice.Id, 1)).ReturnsAsync(invoice);

        var result = await _sut.ExecuteAsync(new DeactivateCardPurchaseRequest(8));

        result.IsSuccess.ShouldBeTrue();
        purchase.Active.ShouldBeFalse();

        pendingSep.Status.ShouldBe("C");
        pendingOct.Status.ShouldBe("C");
        pendingNov.Status.ShouldBe("C");
        openSep.TotalAmount.ShouldBe(0m);
        openOct.TotalAmount.ShouldBe(0m);
        openNov.TotalAmount.ShouldBe(0m);

        settledJul.Status.ShouldBe("Q");
        settledAug.Status.ShouldBe("Q");
        paidJul.TotalAmount.ShouldBe(99.90m);
        paidAug.TotalAmount.ShouldBe(99.90m);
    }

    [Fact]
    public async Task Desativa_a_compra_mesmo_quando_todas_as_faturas_ja_foram_pagas()
    {
        var purchase = Purchase(8, "academia", 199.80m, 2);

        var paidJul = Invoice(15, 2026, 7, 99.90m, paid: true);
        var paidAug = Invoice(16, 2026, 8, 99.90m, paid: true);

        var settledJul = Installment(23, purchase, paidJul, 1, 99.90m, settled: true);
        var settledAug = Installment(24, purchase, paidAug, 2, 99.90m, settled: true);

        _purchaseRepo.Setup(r => r.GetByIdAsync(8, 1)).ReturnsAsync(purchase);
        _purchaseRepo.Setup(r => r.GetInstallmentsByPurchaseAsync(8, 1))
            .ReturnsAsync(new[] { settledJul, settledAug });
        _invoiceRepo.Setup(r => r.GetByIdAsync(15, 1)).ReturnsAsync(paidJul);
        _invoiceRepo.Setup(r => r.GetByIdAsync(16, 1)).ReturnsAsync(paidAug);

        var result = await _sut.ExecuteAsync(new DeactivateCardPurchaseRequest(8));

        result.IsSuccess.ShouldBeTrue();
        purchase.Active.ShouldBeFalse();
        settledJul.Status.ShouldBe("Q");
        settledAug.Status.ShouldBe("Q");
        paidJul.TotalAmount.ShouldBe(99.90m);
        paidAug.TotalAmount.ShouldBe(99.90m);
    }

    private static CardPurchase Purchase(int id, string description, decimal total, short installmentsCount)
    {
        var purchase = new CardPurchase(1, 10, description, total, new DateTime(2026, 7, 1), installmentsCount);
        SetId(purchase, id);
        return purchase;
    }

    private static CardInvoice Invoice(int id, short year, short month, decimal amount, bool paid = false)
    {
        var invoice = new CardInvoice(
            1, 10, year, month, new DateTime(year, month, 1), new DateTime(year, month, 10));
        invoice.AddAmount(amount);
        if (paid)
            invoice.Pay(new Transaction(1, amount, DateTime.UtcNow, "pgto", "E", status: "P"));
        SetId(invoice, id);
        return invoice;
    }

    private static CardInstallment Installment(
        int id, CardPurchase purchase, CardInvoice invoice, short number, decimal amount, bool settled = false)
    {
        var installment = new CardInstallment(1, purchase, invoice, number, amount);
        SetId(installment, id);
        SetProp(installment, nameof(CardInstallment.CardInvoiceId), invoice.Id);
        if (settled)
            installment.Settle();
        return installment;
    }

    private static void SetId(object entity, int id)
        => SetProp(entity, "Id", id);

    private static void SetProp(object entity, string property, object value)
        => entity.GetType().GetProperty(property)!.SetValue(entity, value);
}
