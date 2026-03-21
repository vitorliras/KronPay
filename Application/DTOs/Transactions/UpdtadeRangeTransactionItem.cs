namespace Application.DTOs.Transactions;

public sealed record UpdtadeRangeTransactionItem(
    int Id,
    decimal Amount,
    DateTime TransactionDate,
    string Description,
    string CodTypeTransaction,
    string Status,
    int? CategoryId,
    int? CategoryItemId,
    int IdPaymentMethod
);