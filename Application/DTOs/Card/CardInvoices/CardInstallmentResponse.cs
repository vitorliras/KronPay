namespace Application.DTOs.Card.CardInvoices;

public sealed record CardInstallmentResponse(
    int Id,
    int CardPurchaseId,
    short InstallmentNumber,
    decimal Amount,
    string Status);
