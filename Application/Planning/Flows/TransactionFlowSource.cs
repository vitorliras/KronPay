using Domain.Enums.Planning;
using Domain.Interfaces.Transactions;
using Domain.Models.Planning;

namespace Application.Planning.Flows;

public sealed class TransactionFlowSource : IFinancialFlowSource
{
    private readonly ITransactionRepository _transactions;

    public TransactionFlowSource(ITransactionRepository transactions)
    {
        _transactions = transactions;
    }

    public async Task<IEnumerable<FinancialFlow>> GetFlowsAsync(int userId, DateTime from, DateTime to)
    {
        var transactions = await _transactions.GetByPeriodAsync(userId, from, to);

        return transactions
            .Where(t => t.Status == "O" && (t.CodTypeTransaction == "I" || t.CodTypeTransaction == "E"))
            .Select(t => new FinancialFlow(
                t.TransactionDate,
                MapDirection(t.CodTypeTransaction),
                t.Amount,
                ConfidenceLevel.High,
                FlowOrigin.Transaction,
                t.Description))
            .ToList();
    }

    private static FlowDirection MapDirection(string codTypeTransaction)
        => codTypeTransaction == "I" ? FlowDirection.Inflow : FlowDirection.Outflow;
}
