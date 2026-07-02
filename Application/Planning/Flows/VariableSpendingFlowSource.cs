using Domain.Enums.Planning;
using Domain.Interfaces.Planning;
using Domain.Interfaces.Transactions;
using Domain.Models.Planning;
using Domain.Services.Planning;

namespace Application.Planning.Flows;

public sealed class VariableSpendingFlowSource : IFinancialFlowSource
{
    private const int HistoryMonths = 5;

    private readonly ITransactionRepository _transactions;
    private readonly IPlannedCommitmentRepository _commitments;
    private readonly IVariableSpendingEstimator _estimator;

    public VariableSpendingFlowSource(
        ITransactionRepository transactions,
        IPlannedCommitmentRepository commitments,
        IVariableSpendingEstimator estimator)
    {
        _transactions = transactions;
        _commitments = commitments;
        _estimator = estimator;
    }

    public async Task<IEnumerable<FinancialFlow>> GetFlowsAsync(int userId, DateTime from, DateTime to)
    {
        var firstMonth = new DateTime(from.Year, from.Month, 1);
        var historyStart = firstMonth.AddMonths(-HistoryMonths);

        var past = await _transactions.GetByPeriodAsync(userId, historyStart, firstMonth);
        var realizedExpenses = past
            .Where(t => t.CodTypeTransaction == "E" && t.Status == "P")
            .ToList();

        var fixedMonthlyTotal = await ComputeFixedMonthlyTotalAsync(userId);

        var history = new List<decimal>();
        for (var i = HistoryMonths; i >= 1; i--)
        {
            var month = firstMonth.AddMonths(-i);
            var monthExpenses = realizedExpenses
                .Where(e => e.TransactionDate.Year == month.Year && e.TransactionDate.Month == month.Month)
                .Sum(e => e.Amount);

            if (monthExpenses <= 0)
                continue;

            history.Add(Math.Max(0m, monthExpenses - fixedMonthlyTotal));
        }

        var estimate = _estimator.Estimate(history, fixedMonthlyTotal);
        if (estimate.MonthlyAmount <= 0)
            return Enumerable.Empty<FinancialFlow>();

        var horizon = ((to.Year - firstMonth.Year) * 12) + (to.Month - firstMonth.Month) + 1;

        var flows = new List<FinancialFlow>();
        for (var i = 0; i < horizon; i++)
        {
            flows.Add(new FinancialFlow(
                firstMonth.AddMonths(i),
                FlowDirection.Outflow,
                estimate.MonthlyAmount,
                estimate.Confidence,
                FlowOrigin.VariableEstimate,
                "Gastos variáveis (estimativa)"));
        }

        return flows;
    }

    private async Task<decimal> ComputeFixedMonthlyTotalAsync(int userId)
    {
        var commitments = await _commitments.GetByUserAsync(userId);

        return commitments
            .Where(c => c.Direction == "O")
            .Sum(c => c.Periodicity switch
            {
                "M" => c.Amount,
                "S" => c.Amount * 52m / 12m,
                "A" => c.Amount / 12m,
                _ => 0m
            });
    }
}
