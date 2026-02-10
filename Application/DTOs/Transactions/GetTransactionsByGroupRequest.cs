namespace Application.DTOs.Transactions;

public sealed record GetTransactionsByGroupRequest(
    int UserId,
    int TransactionGroupId
);
