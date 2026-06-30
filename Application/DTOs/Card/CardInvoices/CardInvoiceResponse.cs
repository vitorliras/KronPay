namespace Application.DTOs.Card.CardInvoices;

public sealed record CardInvoiceResponse(
    int Id,
    short ReferenceYear,
    short ReferenceMonth,
    DateTime ClosingDate,
    DateTime DueDate,
    decimal TotalAmount,
    string Status,
    DateTime? PaidAt);
