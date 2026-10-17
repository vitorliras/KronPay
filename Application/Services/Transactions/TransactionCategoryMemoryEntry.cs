namespace Application.Services.Transactions;

public sealed record TransactionCategoryMemoryEntry(
    int TotalOccurrences,
    int BestOccurrences,
    TransactionCategoryMemorySuggestion Best)
{
    public double Dominance => (double)BestOccurrences / TotalOccurrences;
}
