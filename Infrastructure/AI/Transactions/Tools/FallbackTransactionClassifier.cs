using Application.DTOs.Transactions;

namespace Infrastructure.AI.Transactions.Tools;

public sealed class FallbackTransactionClassifier
{
    private static readonly string[] InvestmentKeywords =
    {
        "CDB",
        "TESOURO",
        "LCI",
        "LCA",
        "FII",
        "AÇÃO",
        "ETF",
        "INVEST",
        "XP",
        "RICO",
        "CLEAR",
        "NU INVEST",
        "INTER INVEST"
    };

    public TransactionAiSuggestion Classify(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return new TransactionAiSuggestion(
                IsInvestment: false,
                Confidence: 0m,
                SuggestedCategoryId: null,
                SuggestedType: null
            );
        }

        var normalized = description.ToUpperInvariant();

        var isInvestment = InvestmentKeywords.Any(k => normalized.Contains(k));

        return new TransactionAiSuggestion(
            IsInvestment: isInvestment,
            Confidence: isInvestment ? 0.85m : 0.25m,
            SuggestedCategoryId: null,
            SuggestedType: isInvestment ? "V" : null
        );
    }

    public IReadOnlyList<TransactionAiSuggestion> ClassifyBatch(
        IReadOnlyList<ImportedTransactionResponse> transactions)
    {
        var results = new List<TransactionAiSuggestion>(transactions.Count);

        foreach (var tx in transactions)
        {
            results.Add(Classify(tx.Description));
        }

        return results;
    }
}
