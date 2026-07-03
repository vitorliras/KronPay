using Domain.Models.Planning;

namespace Domain.Services.Planning;

public static class PlanningMetrics
{
    public static decimal MonthlyIncome(FinancialProjection projection)
        => Positive(projection.Months.Count == 0 ? 0m : projection.Months.Average(m => m.Inflows));

    public static decimal MonthlyOutflow(FinancialProjection projection)
        => Positive(projection.Months.Count == 0 ? 0m : projection.Months.Average(m => m.ProbableOutflow));

    private static decimal Positive(decimal value) => value > 0 ? value : 1m;
}
