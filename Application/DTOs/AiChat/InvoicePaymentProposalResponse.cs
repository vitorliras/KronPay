namespace Application.DTOs.AiChat;

public sealed record InvoicePaymentProposalResponse(
    int CardInvoiceId,
    int CreditCardId,
    string CardName,
    short ReferenceYear,
    short ReferenceMonth,
    decimal Amount,
    DateTime DueDate,
    IReadOnlyList<PaymentMethodOptionResponse> PaymentMethods);
