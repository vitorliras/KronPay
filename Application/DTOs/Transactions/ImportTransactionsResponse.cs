public sealed record ImportTransactionsResponse(
    int TotalRead,
    int TotalImported,
    int TotalSkipped,
    IEnumerable<ImportedTransactionResponse> Transactions
);
