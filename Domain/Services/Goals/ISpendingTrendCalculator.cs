using Domain.Enums.Goals;

namespace Domain.Services.Goals;

public interface ISpendingTrendCalculator
{
    SpendingTrendDirection ComputeDirection(IReadOnlyList<decimal> monthlyTotals);
}
