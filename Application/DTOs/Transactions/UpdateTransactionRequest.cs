namespace Application.DTOs.Transactions;

public sealed record UpdateTransactionRequest(
    int Id,
    int UserId,
    string Description,
    decimal Amount,
    DateTime TransactionDate,
    bool UpdateGroup,
    string status,
    int CategoryId,
    int? CategoryItemId,
    DateTime? FromDate            
);
