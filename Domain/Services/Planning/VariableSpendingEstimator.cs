namespace Domain.Services.Planning;

public sealed class VariableSpendingEstimator : IVariableSpendingEstimator
{
    private const int MaxWindow = 6;
    private const decimal MaxTrend = 0.15m;

    public IReadOnlyList<decimal> Estimate(IReadOnlyList<decimal> monthlyHistory, int horizonMonths)
    {
        if (horizonMonths <= 0)
            return Array.Empty<decimal>();

        if (monthlyHistory is null || monthlyHistory.Count == 0)
            return Enumerable.Repeat(0m, horizonMonths).ToList();

        var window = monthlyHistory.TakeLast(MaxWindow).ToList();

        decimal weightedSum = 0m, weightTotal = 0m;
        for (var i = 0; i < window.Count; i++)
        {
            var weight = i + 1;
            weightedSum += window[i] * weight;
            weightTotal += weight;
        }
        var baseline = weightTotal > 0 ? weightedSum / weightTotal : 0m;

        var trend = ComputeTrend(monthlyHistory);
        var estimate = Math.Max(0m, Math.Round(baseline * (1 + trend), 2));

        return Enumerable.Repeat(estimate, horizonMonths).ToList();
    }

    private static decimal ComputeTrend(IReadOnlyList<decimal> history)
    {
        if (history.Count < 4)
            return 0m;

        var half = history.Count / 2;
        var older = history.Take(half).Average();
        var recent = history.Skip(history.Count - half).Average();

        if (older <= 0)
            return 0m;

        var rate = (recent - older) / older;
        return Math.Clamp(rate, -MaxTrend, MaxTrend);
    }
}
