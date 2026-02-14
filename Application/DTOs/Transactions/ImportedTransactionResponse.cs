public sealed record ImportedTransactionResponse(
    DateTime TransactionDate,
    decimal Amount,
    string Description,
    string Type,                 
    string Status,              
    int CategoryId,
    int? CategoryItemId,
    int? TransactionGroupId
);
