using Domain.Enums.Planning;
using Domain.Models.Planning;

namespace Domain.Services.Planning;

public sealed class VariableSpendingEstimator : IVariableSpendingEstimator
{
    private const decimal NoHistoryFactor = 1.5m;
    private const int MediumConfidenceMinMonths = 3;

    public VariableSpendingEstimate Estimate(IReadOnlyList<decimal> variableMonthlyHistory, decimal fixedMonthlyTotal)
    {
        if (variableMonthlyHistory is null || variableMonthlyHistory.Count == 0)
        {
            var fallback = Math.Round(Math.Max(0m, fixedMonthlyTotal) * NoHistoryFactor, 2);
            return new VariableSpendingEstimate(fallback, ConfidenceLevel.Low);
        }

        var average = Math.Round(variableMonthlyHistory.Average(), 2);
        var confidence = variableMonthlyHistory.Count >= MediumConfidenceMinMonths
            ? ConfidenceLevel.Medium
            : ConfidenceLevel.Low;

        return new VariableSpendingEstimate(average, confidence);
    }
}
