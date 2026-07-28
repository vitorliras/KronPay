using Domain.Enums.Planning;
using Domain.Models.Planning;

namespace Domain.Services.Planning;

public sealed class VariableSpendingEstimator : IVariableSpendingEstimator
{
    private const int MediumConfidenceMinMonths = 3;

    public VariableSpendingEstimate Estimate(IReadOnlyList<decimal> signedDeviationHistory)
    {
        if (signedDeviationHistory.Count == 0)
            return new VariableSpendingEstimate(0m, ConfidenceLevel.Low);

        var central = WeightedRecentMedian(signedDeviationHistory);
        var absoluteDeviations = signedDeviationHistory
            .Select(value => Math.Abs(value - central))
            .ToList();
        var dispersion = WeightedRecentMedian(absoluteDeviations);

        var hasEnoughMonths = signedDeviationHistory.Count >= MediumConfidenceMinMonths;
        var isConsistent = dispersion <= Math.Abs(central);

        var confidence = hasEnoughMonths && isConsistent
            ? ConfidenceLevel.Medium
            : ConfidenceLevel.Low;

        return new VariableSpendingEstimate(Math.Round(central, 2), confidence);
    }

    private static decimal WeightedRecentMedian(IReadOnlyList<decimal> monthlyHistoryOldestFirst)
    {
        if (monthlyHistoryOldestFirst.Count == 0)
            return 0m;

        var weighted = new (decimal Value, decimal Weight)[monthlyHistoryOldestFirst.Count];
        decimal totalWeight = 0m;

        for (var i = 0; i < monthlyHistoryOldestFirst.Count; i++)
        {
            var weight = i + 1;
            weighted[i] = (monthlyHistoryOldestFirst[i], weight);
            totalWeight += weight;
        }

        var halfWeight = totalWeight / 2m;
        decimal cumulative = 0m;

        foreach (var (value, weight) in weighted.OrderBy(w => w.Value))
        {
            cumulative += weight;
            if (cumulative >= halfWeight)
                return value;
        }

        return weighted[^1].Value;
    }
}
