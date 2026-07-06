namespace Application.DTOs.Card.CardInvoices;

public sealed record CardInstallmentResponse(
    int Id,
    int CardPurchaseId,
    string PurchaseDescription,
    short InstallmentNumber,
    short InstallmentsCount,
    decimal Amount,
    string Status,
    string? CategoryDescription,
    int? CategoryId,
    int? CategoryItemId);
