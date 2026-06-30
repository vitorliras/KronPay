using Domain.Entities.Card;
using Domain.Interfaces.Card;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Card;

public sealed class CardPurchaseRepository : ICardPurchaseRepository
{
    private readonly AppDbContext _context;

    public CardPurchaseRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> AddAsync(CardPurchase purchase)
    {
        var result = _context.CardPurchases.Add(purchase);
        return result.State == EntityState.Added;
    }

    public async Task<bool> AddInstallmentsAsync(IEnumerable<CardInstallment> installments)
    {
        await _context.CardInstallments.AddRangeAsync(installments);
        return true;
    }

    public async Task<CardPurchase?> GetByIdAsync(int id, int userId)
        => await _context.CardPurchases
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

    public async Task<decimal> SumPendingInstallmentsByCardAsync(int creditCardId, int userId)
        => await _context.CardInstallments
            .AsNoTracking()
            .Where(i => i.UserId == userId && i.Status == "P")
            .Join(
                _context.CardPurchases,
                i => i.CardPurchaseId,
                p => p.Id,
                (i, p) => new { Installment = i, Purchase = p })
            .Where(x => x.Purchase.CreditCardId == creditCardId)
            .SumAsync(x => (decimal?)x.Installment.Amount) ?? 0m;

    // Rastreado: usado no estorno para cancelar parcelas e ajustar as faturas.
    public async Task<IEnumerable<CardInstallment>> GetInstallmentsByPurchaseAsync(int purchaseId, int userId)
        => await _context.CardInstallments
            .Where(x => x.CardPurchaseId == purchaseId && x.UserId == userId)
            .ToListAsync();
}
