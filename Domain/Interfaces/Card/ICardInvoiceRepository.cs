using Domain.Entities.Card;

namespace Domain.Interfaces.Card;

public interface ICardInvoiceRepository
{
    Task<CardInvoice?> GetByCycleAsync(int creditCardId, int userId, short referenceYear, short referenceMonth);
    Task<bool> AddAsync(CardInvoice invoice);
    bool Update(CardInvoice invoice);
    Task<CardInvoice?> GetByIdAsync(int id, int userId);
    Task<IEnumerable<CardInvoice>> GetByCardAsync(int creditCardId, int userId);
    Task<IEnumerable<CardInstallment>> GetInstallmentsByInvoiceAsync(int invoiceId, int userId);
    Task<IEnumerable<CardInstallment>> GetPendingInstallmentsByInvoiceAsync(int invoiceId, int userId);
}
