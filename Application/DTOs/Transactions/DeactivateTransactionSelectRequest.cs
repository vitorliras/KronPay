using Application.DTOs.Configuration.Categories;

namespace Application.DTOs.Transactions;

public sealed record DeactivateTransactionSelectRequest(
    IReadOnlyCollection<DeactivateTransactionRequest> Transactions
);
