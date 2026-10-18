using Domain.Entities.Card;
using Domain.Interfaces.Card;

namespace Application.Services.Card;

public sealed class CardInvoiceReconciler
{
    private const string PendingStatus = "P";
    private const string SettledStatus = "Q";

    private readonly ICardInvoiceRepository _invoiceRepository;

    public CardInvoiceReconciler(ICardInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<bool> RestorePaidWhenFullySettledAsync(
        CardInvoice invoice,
        int userId,
        IReadOnlyCollection<int> cancelledInstallmentIds)
    {
        if (invoice.IsPaid)
            return false;

        var remaining = (await _invoiceRepository.GetInstallmentsByInvoiceAsync(invoice.Id, userId))
            .Where(i => !cancelledInstallmentIds.Contains(i.Id))
            .ToList();

        if (remaining.Any(i => i.Status == PendingStatus) || !remaining.Any(i => i.Status == SettledStatus))
            return false;

        invoice.RestoreAsPaid();
        return true;
    }
}
