namespace Application.DTOs.Transactions;

public sealed record ChangeStatusTransactionRequest(
    int Id,
    string Status
);
