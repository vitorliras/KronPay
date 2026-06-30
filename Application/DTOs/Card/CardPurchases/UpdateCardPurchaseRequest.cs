namespace Application.DTOs.Card.CardPurchases;

public sealed record UpdateCardPurchaseRequest(
    int Id,
    string Description,
    int? CategoryId = null,
    int? CategoryItemId = null);
