namespace Application.Services.Spending;

public sealed record LifetimeTotals(DateTime FirstDate, IReadOnlyList<YearlyTotal> Years)
{
    public decimal Expenses => Years.Sum(y => y.Expenses);
    public decimal Income => Years.Sum(y => y.Income);
    public decimal Balance => Income - Expenses;
}
