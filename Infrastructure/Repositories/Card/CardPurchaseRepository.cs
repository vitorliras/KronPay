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

    public async Task<IEnumerable<CardInstallment>> GetInstallmentsByPurchaseAsync(int purchaseId, int userId)
        => await _context.CardInstallments
            .Where(x => x.CardPurchaseId == purchaseId && x.UserId == userId)
            .ToListAsync();

    public async Task<IReadOnlyList<CardPurchase>> GetDeactivatedOlderThanAsync(DateTime cutoff)
    {
        return await _context.CardPurchases
            .Where(x => !x.Active && x.DeactivatedAt != null && x.DeactivatedAt < cutoff)
            .ToListAsync();
    }

    public Task DeleteRangeAsync(IEnumerable<CardPurchase> purchases)
    {
        _context.CardPurchases.RemoveRange(purchases);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsInstallmentByCardPurchaseIdAsync(int cardPurchaseId)
    {
        return await _context.CardInstallments
            .AsNoTracking()
            .AnyAsync(x => x.CardPurchaseId == cardPurchaseId);
    }

    public async Task<bool> ExistsByCreditCardIdAsync(int creditCardId)
    {
        return await _context.CardPurchases
            .AsNoTracking()
            .AnyAsync(x => x.CreditCardId == creditCardId);
    }

    public async Task<bool> ExistsByCategoryIdAsync(int categoryId)
    {
        return await _context.CardPurchases
            .AsNoTracking()
            .AnyAsync(x => x.CategoryId == categoryId);
    }

    public async Task<bool> ExistsByCategoryItemIdAsync(int categoryItemId)
    {
        return await _context.CardPurchases
            .AsNoTracking()
            .AnyAsync(x => x.CategoryItemId == categoryItemId);
    }
}
