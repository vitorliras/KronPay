namespace Application.DTOs.Card.CardPurchases;

public sealed record CreateCardPurchaseRangeRequest(
    int CreditCardId,
    IReadOnlyList<CreateCardPurchaseRangeItem> Purchases);
