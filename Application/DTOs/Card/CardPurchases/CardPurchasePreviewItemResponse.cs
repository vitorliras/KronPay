namespace Application.DTOs.Card.CardPurchases;

public sealed record CardPurchasePreviewItemResponse(
    DateTime PurchaseDate,
    string Description,
    decimal TotalAmount,
    short InstallmentsCount,
    int? CategoryId,
    int? CategoryItemId,
    string? ClassificationSource,
    bool PossibleDuplicate);
