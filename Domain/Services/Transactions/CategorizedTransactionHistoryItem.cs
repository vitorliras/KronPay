namespace Domain.Services.Transactions;

public sealed record CategorizedTransactionHistoryItem(
    string Description,
    string Type,
    int CategoryId,
    int? CategoryItemId,
    int Occurrences);
