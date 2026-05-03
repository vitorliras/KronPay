namespace Application.DTOs.Transactions;

public sealed record GetTransactionsByDatesRequest(
    DateTime StartDate,
    DateTime ? EndDate
);

