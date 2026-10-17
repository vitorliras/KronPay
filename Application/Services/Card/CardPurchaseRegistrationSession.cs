using Domain.Entities.Card;
using Domain.Interfaces.Card;
using Domain.Services.Card;

namespace Application.Services.Card;

public sealed class CardPurchaseRegistrationSession
{
    private readonly CreditCard _card;
    private readonly int _userId;
    private readonly ICardPurchaseRepository _purchaseRepository;
    private readonly ICardInvoiceRepository _invoiceRepository;
    private readonly ICreditCardBillingCalculator _billingCalculator;
    private readonly Dictionary<(short Year, short Month), CardInvoice> _invoicesByCycle = new();

    internal CardPurchaseRegistrationSession(
        CreditCard card,
        int userId,
        ICardPurchaseRepository purchaseRepository,
        ICardInvoiceRepository invoiceRepository,
        ICreditCardBillingCalculator billingCalculator)
    {
        _card = card;
        _userId = userId;
        _purchaseRepository = purchaseRepository;
        _invoiceRepository = invoiceRepository;
        _billingCalculator = billingCalculator;
    }

    public async Task<CardPurchase?> RegisterAsync(
        string description,
        decimal totalAmount,
        DateTime purchaseDate,
        short installmentsCount,
        int? categoryId,
        int? categoryItemId)
    {
        var purchase = new CardPurchase(
            _userId,
            _card.Id,
            description,
            totalAmount,
            purchaseDate,
            installmentsCount,
            categoryId,
            categoryItemId);

        if (!await _purchaseRepository.AddAsync(purchase))
            return null;

        var amounts = SplitInstallments(totalAmount, installmentsCount);
        var installments = new List<CardInstallment>();

        for (short i = 0; i < installmentsCount; i++)
        {
            var cycle = _billingCalculator.Resolve(_card, purchaseDate.AddMonths(i));

            var invoice = await GetOrCreateInvoiceAsync(cycle);
            if (invoice is null)
                return null;

            invoice.AddAmount(amounts[i]);

            installments.Add(new CardInstallment(
                _userId,
                purchase,
                invoice,
                (short)(i + 1),
                amounts[i]));
        }

        return await _purchaseRepository.AddInstallmentsAsync(installments) ? purchase : null;
    }

    private async Task<CardInvoice?> GetOrCreateInvoiceAsync(BillingCycle cycle)
    {
        var key = (cycle.ReferenceYear, cycle.ReferenceMonth);

        if (_invoicesByCycle.TryGetValue(key, out var cached))
            return cached;

        var invoice = await _invoiceRepository.GetByCycleAsync(
            _card.Id, _userId, cycle.ReferenceYear, cycle.ReferenceMonth);

        if (invoice is null)
        {
            invoice = new CardInvoice(
                _userId,
                _card.Id,
                cycle.ReferenceYear,
                cycle.ReferenceMonth,
                cycle.ClosingDate,
                cycle.DueDate);

            if (!await _invoiceRepository.AddAsync(invoice))
                return null;
        }
        else if (invoice.IsPaid)
        {
            invoice.Reopen();
        }

        _invoicesByCycle[key] = invoice;
        return invoice;
    }

    private static decimal[] SplitInstallments(decimal total, short count)
    {
        var amounts = new decimal[count];
        var perInstallment = Math.Floor(total / count * 100m) / 100m;
        decimal accumulated = 0m;

        for (var i = 0; i < count - 1; i++)
        {
            amounts[i] = perInstallment;
            accumulated += perInstallment;
        }

        amounts[count - 1] = total - accumulated;
        return amounts;
    }
}
