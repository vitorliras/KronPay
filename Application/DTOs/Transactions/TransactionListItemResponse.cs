namespace Application.DTOs.Transactions;

public sealed record GetTransactionsByMonthRequest(
    int Year,
    int Month
);

