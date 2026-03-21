namespace Application.DTOs.Transactions;

public sealed record UpdtadeRangeTransaction(
    IReadOnlyCollection<UpdtadeRangeTransactionItem> Transactions
);
