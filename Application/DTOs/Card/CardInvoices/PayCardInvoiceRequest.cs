namespace Application.DTOs.Card.CardInvoices;

public sealed record PayCardInvoiceRequest(
    int CardInvoiceId,
    int PaymentMethodId,
    string CodTypeTransaction);
