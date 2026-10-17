namespace Application.Services.Transactions;

public sealed record TransactionCategoryMemorySuggestion(
    string Type,
    int CategoryId,
    int? CategoryItemId);
