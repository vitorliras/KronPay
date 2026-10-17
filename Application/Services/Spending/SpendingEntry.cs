namespace Application.Services.Spending;

public sealed record SpendingEntry(
    DateTime Date,
    string Description,
    decimal Amount,
    int? CategoryId,
    int? CategoryItemId,
    bool FromCard,
    int? CreditCardId);
