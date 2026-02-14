namespace Application.DTOs.Transactions;

public sealed record TransactionRangeResponse(
    int AffectedTransactions,
    string message,
    bool Success
);