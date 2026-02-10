namespace Application.DTOs.Transactions;

public sealed record TransactionResponse(
    int? Id,
    string? Description,
    int? TransactionGroupId,
    int? AffectedTransactions,
    string message,
    bool Success
);