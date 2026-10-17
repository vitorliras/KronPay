using Domain.Entities.Card;
using Domain.Interfaces.Card;
using Domain.Services.Card;

namespace Application.Services.Card;

public sealed class CardPurchaseRegistrar
{
    private readonly ICardPurchaseRepository _purchaseRepository;
    private readonly ICardInvoiceRepository _invoiceRepository;
    private readonly ICreditCardBillingCalculator _billingCalculator;

    public CardPurchaseRegistrar(
        ICardPurchaseRepository purchaseRepository,
        ICardInvoiceRepository invoiceRepository,
        ICreditCardBillingCalculator billingCalculator)
    {
        _purchaseRepository = purchaseRepository;
        _invoiceRepository = invoiceRepository;
        _billingCalculator = billingCalculator;
    }

    public CardPurchaseRegistrationSession StartSession(CreditCard card, int userId) =>
        new(card, userId, _purchaseRepository, _invoiceRepository, _billingCalculator);
}
