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

    public async Task<IEnumerable<CardInstallment>> GetInstallmentsByInvoiceAsync(int invoiceId, int userId)
        => await _context.CardInstallments
            .AsNoTracking()
            .Include(x => x.CardPurchase)
            .Where(x => x.CardInvoiceId == invoiceId && x.UserId == userId)
            .OrderBy(x => x.InstallmentNumber)
            .ToListAsync();

    // Sem AsNoTracking: as parcelas precisam ser rastreadas para a quitação (Settle) persistir.
    public async Task<IEnumerable<CardInstallment>> GetPendingInstallmentsByInvoiceAsync(int invoiceId, int userId)
        => await _context.CardInstallments
            .Where(x => x.CardInvoiceId == invoiceId && x.UserId == userId && x.Status == "P")
            .ToListAsync();
}
