namespace Application.DTOs.Card.CardPurchases;

public sealed record CardPurchasePreviewResponse(
    int TotalRead,
    int Skipped,
    IReadOnlyList<CardPurchasePreviewItemResponse> Purchases);
