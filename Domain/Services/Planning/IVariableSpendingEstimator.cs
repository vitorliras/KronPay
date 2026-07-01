namespace Domain.Services.Planning;

public interface IVariableSpendingEstimator
{
    IReadOnlyList<decimal> Estimate(IReadOnlyList<decimal> monthlyHistory, int horizonMonths);
}
