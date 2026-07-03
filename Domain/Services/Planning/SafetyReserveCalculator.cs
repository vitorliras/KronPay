using Domain.Models.Planning;

namespace Domain.Services.Planning;

public sealed class SafetyReserveCalculator : ISafetyReserveCalculator
{
    private const decimal MinReserve = 100m;

    public decimal Calculate(FinancialProjection projection)
    {
        if (projection.Months.Count == 0)
            return MinReserve;

        var averageMonthlyOutflow = projection.Months.Average(m => m.ProbableOutflow);

        return Math.Max(Math.Round(averageMonthlyOutflow, 2), MinReserve);
    }
}
