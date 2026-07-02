using Domain.Models.Planning;

namespace Domain.Services.Planning;

public interface IVariableSpendingEstimator
{
    VariableSpendingEstimate Estimate(IReadOnlyList<decimal> variableMonthlyHistory, decimal fixedMonthlyTotal);
}
