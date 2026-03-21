namespace Application.DTOs.Transactions;

public sealed record ChangeStatusTransactionRangeRequest(
    IReadOnlyCollection<ChangeStatusTransactionRequest> Transactions
);
