public sealed record ImportedTransactionDto(
    int UserId,
    decimal Amount,
    DateTime TransactionDate,
    string Description,
    string CodTypeTransaction,
    int CategoryId,
    int? CategoryItemId
);