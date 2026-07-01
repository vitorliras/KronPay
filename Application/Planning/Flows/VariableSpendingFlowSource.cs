using Domain.Enums.Planning;
using Domain.Interfaces.Transactions;
using Domain.Models.Planning;
using Domain.Services.Planning;

namespace Application.Planning.Flows;

/// <summary>
/// Estima o gasto variável futuro a partir das despesas realizadas dos últimos meses e o
/// materializa em fluxos de saída com confiança Medium (ou Low quando há pouco histórico).
/// É esta fonte que ativa a banda de confiança da projeção.
/// </summary>
/// <remarks>
/// Limitação conhecida (ADR 0013): sem classificação fixa×variável no histórico, esta
/// estimativa pode sobrepor-se a despesas fixas (compromissos) ou a transações futuras já
/// registradas. Refinamento previsto para elevar a precisão.
/// </remarks>
public sealed class VariableSpendingFlowSource : IFinancialFlowSource
{
    private const int HistoryMonths = 6;

    private readonly ITransactionRepository _transactions;
    private readonly IVariableSpendingEstimator _estimator;

    public VariableSpendingFlowSource(
        ITransactionRepository transactions,
        IVariableSpendingEstimator estimator)
    {
        _transactions = transactions;
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

        var history = new List<decimal>();
        for (var i = HistoryMonths; i >= 1; i--)
        {
            var month = firstMonth.AddMonths(-i);
            var total = realizedExpenses
                .Where(e => e.TransactionDate.Year == month.Year && e.TransactionDate.Month == month.Month)
                .Sum(e => e.Amount);
            history.Add(total);
        }

        var monthsWithData = history.Count(h => h > 0);
        if (monthsWithData == 0)
            return Enumerable.Empty<FinancialFlow>();

        var horizon = ((to.Year - firstMonth.Year) * 12) + (to.Month - firstMonth.Month) + 1;
        var estimates = _estimator.Estimate(history, horizon);
        var confidence = monthsWithData >= 3 ? ConfidenceLevel.Medium : ConfidenceLevel.Low;

        var flows = new List<FinancialFlow>();
        for (var i = 0; i < estimates.Count; i++)
        {
            if (estimates[i] <= 0)
                continue;

            flows.Add(new FinancialFlow(
                firstMonth.AddMonths(i),
                FlowDirection.Outflow,
                estimates[i],
                confidence,
                FlowOrigin.VariableEstimate,
                "Gastos variáveis (estimativa)"));
        }

        return flows;
    }
}
