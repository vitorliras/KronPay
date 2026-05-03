namespace Application.DTOs.Transactions;

public sealed record UpdateTransactionRequest(
    int Id,
    string Description,
    string type,
    decimal Amount,
    DateTime TransactionDate,
    bool UpdateGroup,
    string status,
    int? CategoryId,
    int? CategoryItemId
);
