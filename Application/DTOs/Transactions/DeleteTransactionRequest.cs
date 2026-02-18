namespace Application.DTOs.Transactions;

public sealed record DeleteTransactionRequest(
    int TransactionId,
    bool DeleteGroup,           
    DateTime? FromDate           
);
