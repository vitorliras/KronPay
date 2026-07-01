namespace Domain.Models.Planning;

public sealed record FinancialProjection(IReadOnlyList<ProjectionMonth> Months)
{
    public decimal FinalBalance => Months.Count > 0 ? Months[^1].ClosingBalance : 0m;

    public ProjectionMonth? FirstNegativeMonth
        => Months.FirstOrDefault(m => m.PessimisticClosing < 0);
}
