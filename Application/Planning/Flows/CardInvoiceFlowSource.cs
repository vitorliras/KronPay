using Domain.Enums.Planning;
using Domain.Interfaces.Card;
using Domain.Models.Planning;

namespace Application.Planning.Flows;

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
        var pending = await _invoices.GetPendingAmountsByUserAsync(userId) ?? new Dictionary<int, decimal>();

        return invoices
            .Where(i => i.Status != "P" && pending.GetValueOrDefault(i.Id) > 0)
            .Select(i => new FinancialFlow(
                i.DueDate.Date < from.Date ? from.Date : i.DueDate,
                FlowDirection.Outflow,
                pending.GetValueOrDefault(i.Id),
                ConfidenceLevel.High,
                FlowOrigin.CardInvoice,
                $"Fatura {i.ReferenceMonth}/{i.ReferenceYear}"))
            .ToList();
    }
}
