public sealed record ImportedTransactionResponse(
    DateTime TransactionDate,
    decimal Amount,
    string Description,
    string Type,                 
    string Status,              
    int PaymentMethod,
    int? CategoryId,
    int? CategoryItemId,
    int? TransactionGroupId,
    int? Id
);
