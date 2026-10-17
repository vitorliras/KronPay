using Domain.Entities.Card;
using Domain.Interfaces.Card;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Card;

public sealed class CardInvoiceRepository : ICardInvoiceRepository
{
    private readonly AppDbContext _context;

    public CardInvoiceRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CardInvoice?> GetByCycleAsync(int creditCardId, int userId, short referenceYear, short referenceMonth)
        => await _context.CardInvoices
            .FirstOrDefaultAsync(x => x.CreditCardId == creditCardId
                && x.UserId == userId
                && x.ReferenceYear == referenceYear
                && x.ReferenceMonth == referenceMonth);

    public async Task<bool> AddAsync(CardInvoice invoice)
    {
        var result = _context.CardInvoices.Add(invoice);
        return result.State == EntityState.Added;
    }

    public bool Update(CardInvoice invoice)
    {
        var result = _context.CardInvoices.Update(invoice);
        return result.State == EntityState.Modified;
    }

    public async Task<CardInvoice?> GetByIdAsync(int id, int userId)
        => await _context.CardInvoices
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

    public async Task<IEnumerable<CardInvoice>> GetByCardAsync(int creditCardId, int userId)
        => await _context.CardInvoices
            .AsNoTracking()
            .Where(x => x.CreditCardId == creditCardId && x.UserId == userId)
            .OrderByDescending(x => x.ReferenceYear)
            .ThenByDescending(x => x.ReferenceMonth)
            .ToListAsync();

    public async Task<IEnumerable<CardInvoice>> GetByUserAsync(int userId)
        => await _context.CardInvoices
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.ReferenceYear)
            .ThenBy(x => x.ReferenceMonth)
            .ToListAsync();

    public async Task<IEnumerable<CardInstallment>> GetInstallmentsByInvoiceAsync(int invoiceId, int userId)
        => await _context.CardInstallments
            .AsNoTracking()
            .Include(x => x.CardPurchase)
            .Where(x => x.CardInvoiceId == invoiceId && x.UserId == userId)
            .OrderBy(x => x.InstallmentNumber)
            .ToListAsync();

    public async Task<IEnumerable<CardInstallment>> GetPendingInstallmentsByInvoiceAsync(int invoiceId, int userId)
        => await _context.CardInstallments
            .Where(x => x.CardInvoiceId == invoiceId && x.UserId == userId && x.Status == "P")
            .ToListAsync();

    public async Task<IReadOnlyDictionary<int, decimal>> GetPendingAmountsByCardAsync(int creditCardId, int userId)
        => await (
            from installment in _context.CardInstallments
            join invoice in _context.CardInvoices on installment.CardInvoiceId equals invoice.Id
            where installment.UserId == userId
                  && invoice.CreditCardId == creditCardId
                  && installment.Status == "P"
            group installment by installment.CardInvoiceId into byInvoice
            select new { InvoiceId = byInvoice.Key, Total = byInvoice.Sum(x => x.Amount) })
            .AsNoTracking()
            .ToDictionaryAsync(x => x.InvoiceId, x => x.Total);

    public async Task<IReadOnlyCollection<int>> GetPaymentTransactionIdsAsync(int userId)
        => await _context.CardInvoices
            .AsNoTracking()
            .Where(i => i.UserId == userId && i.TransactionId != null)
            .Select(i => i.TransactionId!.Value)
            .ToListAsync();

    public async Task<IReadOnlyList<CardInstallmentDetail>> GetInstallmentDetailsAsync(
        int userId,
        int fromYear,
        int fromMonth,
        int toYear,
        int toMonth)
    {
        var fromIndex = fromYear * 12 + fromMonth;
        var toIndex = toYear * 12 + toMonth;

        return await (
            from installment in _context.CardInstallments
            join invoice in _context.CardInvoices on installment.CardInvoiceId equals invoice.Id
            join purchase in _context.CardPurchases on installment.CardPurchaseId equals purchase.Id
            where installment.UserId == userId
                  && purchase.Active
                  && installment.Status != "C"
                  && invoice.ReferenceYear * 12 + invoice.ReferenceMonth >= fromIndex
                  && invoice.ReferenceYear * 12 + invoice.ReferenceMonth <= toIndex
            select new CardInstallmentDetail(
                invoice.ReferenceYear,
                invoice.ReferenceMonth,
                purchase.Description,
                installment.Amount,
                purchase.CategoryId,
                purchase.CategoryItemId,
                purchase.CreditCardId,
                purchase.PurchaseDate))
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<bool> ExistsByCreditCardIdAsync(int creditCardId)
    {
        return await _context.CardInvoices
            .AsNoTracking()
            .AnyAsync(x => x.CreditCardId == creditCardId);
    }
}
