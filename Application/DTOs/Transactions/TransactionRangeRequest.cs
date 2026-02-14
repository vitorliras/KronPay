using Domain.Entities.Transactions;

namespace Application.DTOs.Transactions;

public sealed record TransactionRangeRequest(
    IReadOnlyCollection<Transaction> Transactions
);
