namespace Application.DTOs.Transactions;

public sealed record GetTransactionsByGroupRequest(
    int TransactionGroupId
);
