namespace Application.DTOs.Transactions;

public sealed record GetTransactionsByYearRequest(
    int UserId,
    int Year
);

