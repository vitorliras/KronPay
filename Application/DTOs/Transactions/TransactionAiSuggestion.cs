
namespace Application.DTOs.Transactions
{
    public sealed record TransactionAiSuggestion(
        bool IsInvestment,
        decimal Confidence,
        int? SuggestedCategoryId,
        string? SuggestedType
    );
}
