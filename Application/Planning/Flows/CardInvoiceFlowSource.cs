using Domain.Enums.Planning;
using Domain.Interfaces.Card;
using Domain.Models.Planning;

namespace Application.Planning.Flows;

/// <summary>
/// Traduz faturas <b>não pagas</b> em saídas comprometidas (vencimento como competência).
/// Faturas pagas viraram uma <c>Transaction</c> realizada (entra no saldo inicial), então
/// são ignoradas aqui — evitando dupla contagem cartão↔fatura. Faturas vencidas e ainda em
/// aberto são ancoradas no início da janela para não se perderem.
/// </summary>
public sealed class CardInvoiceFlowSource : IFinancialFlowSource
{
    private readonly ICardInvoiceRepository _invoices;

    public CardInvoiceFlowSource(ICardInvoiceRepository invoices)
    {
        _invoices = invoices;
    }

    public async Task<IEnumerable<FinancialFlow>> GetFlowsAsync(int userId, DateTime from, DateTime to)
    {
        var invoices = await _invoices.GetByUserAsync(userId);

        return invoices
            .Where(i => i.Status != "P" && i.TotalAmount > 0)
            .Select(i => new FinancialFlow(
                i.DueDate.Date < from.Date ? from.Date : i.DueDate,
                FlowDirection.Outflow,
                i.TotalAmount,
                ConfidenceLevel.High,
                FlowOrigin.CardInvoice,
                $"Fatura {i.ReferenceMonth}/{i.ReferenceYear}"))
            .ToList();
    }
}
