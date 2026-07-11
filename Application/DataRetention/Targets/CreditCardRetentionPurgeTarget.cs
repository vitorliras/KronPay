using Domain.Entities.Card;
using Domain.Interfaces;
using Domain.Interfaces.Card;

namespace Application.DataRetention.Targets;

public sealed class CreditCardRetentionPurgeTarget : IRetentionPurgeTarget
{
    private readonly ICreditCardRepository _creditCards;
    private readonly ICardPurchaseRepository _cardPurchases;
    private readonly ICardInvoiceRepository _cardInvoices;

    public CreditCardRetentionPurgeTarget(
        ICreditCardRepository creditCards,
        ICardPurchaseRepository cardPurchases,
        ICardInvoiceRepository cardInvoices)
    {
        _creditCards = creditCards;
        _cardPurchases = cardPurchases;
        _cardInvoices = cardInvoices;
    }

    public async Task<int> PurgeAsync(DateTime cutoff)
    {
        var candidates = await _creditCards.GetDeactivatedOlderThanAsync(cutoff);
        if (candidates.Count == 0)
            return 0;

        var deletable = new List<CreditCard>();

        foreach (var card in candidates)
        {
            var referencedByPurchase = await _cardPurchases.ExistsByCreditCardIdAsync(card.Id);
            var referencedByInvoice = await _cardInvoices.ExistsByCreditCardIdAsync(card.Id);

            if (!referencedByPurchase && !referencedByInvoice)
                deletable.Add(card);
        }

        if (deletable.Count == 0)
            return 0;

        await _creditCards.DeleteRangeAsync(deletable);
        return deletable.Count;
    }
}
