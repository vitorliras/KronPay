namespace Application.DTOs.Transactions;

public sealed record ChangeStatusTransactionRequest(
    int Id,
    int UserId,
    string Status
);
