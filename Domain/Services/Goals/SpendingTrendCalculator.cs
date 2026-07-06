using Domain.Enums.Goals;

namespace Domain.Services.Goals;

public sealed class SpendingTrendCalculator : ISpendingTrendCalculator
{
    private const decimal TrendMarginPercent = 0.10m;

    public SpendingTrendDirection ComputeDirection(IReadOnlyList<decimal> monthlyTotals)
    {
        var window = monthlyTotals.TakeLast(3).ToList();
        if (window.Count < 2)
            return SpendingTrendDirection.Stable;

        var oldest = window.First();
        var newest = window.Last();

        if (oldest <= 0)
            return newest > 0 ? SpendingTrendDirection.Rising : SpendingTrendDirection.Stable;

        if (newest > oldest * (1 + TrendMarginPercent))
            return SpendingTrendDirection.Rising;

        if (newest < oldest * (1 - TrendMarginPercent))
            return SpendingTrendDirection.Falling;

        return SpendingTrendDirection.Stable;
    }
}
