namespace Domain.Interfaces.Card;

public sealed record CardInstallmentDetail(
    short InvoiceYear,
    short InvoiceMonth,
    string Description,
    decimal Amount,
    int? CategoryId,
    int? CategoryItemId,
    int CreditCardId,
    DateTime PurchaseDate);
