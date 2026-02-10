namespace Application.DTOs.Transactions;

public sealed record TransactionListItemResponse(
    int Id,
    string Description,
    decimal Amount,
    DateTime TransactionDate,
    string CodTypeTransaction,
    string Status
);

