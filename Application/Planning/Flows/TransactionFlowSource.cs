using Domain.Enums.Planning;
using Domain.Interfaces.Transactions;
using Domain.Models.Planning;

namespace Application.Planning.Flows;

/// <summary>
/// Traduz transações <b>previstas</b> (status "O" = em aberto) em fluxos comprometidos.
/// Transações realizadas (pagas) já compõem o saldo inicial; pagamentos de fatura são
/// sempre gerados no presente com status "P", logo não aparecem aqui — evitando dupla contagem.
/// </summary>
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
            .Where(t => t.Status == "O")
            .Select(t => new FinancialFlow(
                t.TransactionDate,
                MapDirection(t.CodTypeTransaction),
                t.Amount,
                ConfidenceLevel.High,
                FlowOrigin.Transaction,
                t.Description))
            .ToList();
    }

    // I = receita (entrada); E = despesa e V = investimento saem do caixa (saída).
    private static FlowDirection MapDirection(string codTypeTransaction)
        => codTypeTransaction == "I" ? FlowDirection.Inflow : FlowDirection.Outflow;
}
