namespace Application.DTOs.Card.CardPurchases;

public sealed record CreateCardPurchaseRangeItem(
    string Description,
    decimal TotalAmount,
    DateTime PurchaseDate,
    short InstallmentsCount,
    int? CategoryId = null,
    int? CategoryItemId = null);
