using Domain.Entities.Planning;
using Domain.Entities.Transactions;
using Domain.Enums.Planning;
using Domain.Interfaces.Planning;
using Domain.Interfaces.Transactions;
using Domain.Models.Planning;
using Domain.Services.Planning;

namespace Application.Planning.Flows;

public sealed class VariableSpendingFlowSource : IFinancialFlowSource
{
    private const int HistoryMonths = 12;

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

        var commitments = (await _commitments.GetByUserAsync(userId)).ToList();
        var expenseCommitments = commitments.Where(c => c.Direction == "O").ToList();
        var incomeCommitments = commitments.Where(c => c.Direction == "I").ToList();

        var past = await _transactions.GetByPeriodAsync(userId, historyStart, firstMonth);
        var pastList = past.ToList();

        var realizedExpenses = pastList.Where(t => t.CodTypeTransaction == "E" && t.Status == "P").ToList();
        var realizedIncome = pastList.Where(t => t.CodTypeTransaction == "I" && t.Status == "P").ToList();

        var netResultHistory = BuildNetResultHistory(realizedExpenses, realizedIncome, historyStart, firstMonth);
        if (netResultHistory.Count == 0)
            return Enumerable.Empty<FinancialFlow>();

        var netEstimate = _estimator.Estimate(netResultHistory);
        var confidenceWeight = netEstimate.Confidence == ConfidenceLevel.Low ? 0.5m : 1.0m;
        var halfLife = Math.Max(1, netResultHistory.Count);
        var horizon = ((to.Year - firstMonth.Year) * 12) + (to.Month - firstMonth.Month) + 1;

        var flows = new List<FinancialFlow>();

        for (var i = 0; i < horizon; i++)
        {
            var competence = firstMonth.AddMonths(i);
            var committedNet = CommittedTotalForMonth(incomeCommitments, competence) - CommittedTotalForMonth(expenseCommitments, competence);
            var gap = Math.Max(0m, committedNet - netEstimate.CentralDeviation) * confidenceWeight;

            if (gap <= 0)
                continue;

            var decayed = Math.Round(gap * DecayFactor(i, halfLife), 2);
            if (decayed > 0)
                flows.Add(new FinancialFlow(
                    competence,
                    FlowDirection.Outflow,
                    decayed,
                    netEstimate.Confidence,
                    FlowOrigin.VariableEstimate,
                    "Ajuste com base no resultado líquido histórico (estimativa)"));
        }

        return flows;
    }

    private static List<decimal> BuildNetResultHistory(
        IReadOnlyList<Transaction> realizedExpenses,
        IReadOnlyList<Transaction> realizedIncome,
        DateTime historyStart,
        DateTime firstMonth)
    {
        var history = new List<decimal>();
        var month = new DateTime(historyStart.Year, historyStart.Month, 1);

        while (month < firstMonth)
        {
            var expenseTotal = realizedExpenses
                .Where(t => t.TransactionDate.Year == month.Year && t.TransactionDate.Month == month.Month)
                .Sum(t => t.Amount);

            var incomeTotal = realizedIncome
                .Where(t => t.TransactionDate.Year == month.Year && t.TransactionDate.Month == month.Month)
                .Sum(t => t.Amount);

            if (expenseTotal > 0 || incomeTotal > 0)
                history.Add(incomeTotal - expenseTotal);

            month = month.AddMonths(1);
        }

        return history;
    }

    private static decimal DecayFactor(int monthsAhead, int halfLifeMonths)
    {
        var exponent = (double)monthsAhead / halfLifeMonths;
        return (decimal)Math.Pow(0.5, exponent);
    }

    private static decimal CommittedTotalForMonth(IReadOnlyList<PlannedCommitment> commitments, DateTime month)
    {
        var monthEnd = month.AddMonths(1).AddDays(-1);

        return commitments
            .Where(c => c.StartDate.Date <= monthEnd && (c.EndDate is null || c.EndDate.Value.Date >= month))
            .Sum(c => c.Periodicity switch
            {
                "M" => c.Amount,
                "S" => c.Amount * 52m / 12m,
                "A" => c.Amount / 12m,
                _ => 0m
            });
    }
}
