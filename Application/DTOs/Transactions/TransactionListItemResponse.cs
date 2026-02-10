namespace Application.DTOs.Transactions;

public sealed record GetTransactionsByMonthRequest(
    int UserId,
    int Year,
    int Month
);

