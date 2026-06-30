namespace Application.DTOs.Card.CardInvoices;

public sealed record CreditCardSummaryResponse(
    int CreditCardId,
    decimal CreditLimit,
    decimal UsedAmount,
    decimal AvailableAmount);
