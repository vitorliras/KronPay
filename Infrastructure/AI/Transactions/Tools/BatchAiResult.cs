namespace Infrastructure.AI.Transactions.Tools;

public sealed class BatchAiResult
{
    public int Index { get; set; }
    public bool IsInvestment { get; set; }
    public decimal Confidence { get; set; }
    public int? SuggestedCategoryId { get; set; }
    public string? SuggestedType { get; set; }
}
