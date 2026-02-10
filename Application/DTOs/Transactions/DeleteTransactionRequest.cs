namespace Application.DTOs.Transactions;

public sealed record DeleteTransactionRequest(
    int UserId,
    int TransactionId,
    bool DeleteGroup,           
    DateTime? FromDate           
);
