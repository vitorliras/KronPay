namespace Application.DTOs.Card.CardPurchases;

public sealed record CardPurchaseResponse(
    int Id,
    string Description,
    decimal TotalAmount,
    short InstallmentsCount);
