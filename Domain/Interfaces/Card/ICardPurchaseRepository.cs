using Domain.Entities.Card;

namespace Domain.Interfaces.Card;

public interface ICardPurchaseRepository
{
    Task<bool> AddAsync(CardPurchase purchase);
    Task<bool> AddInstallmentsAsync(IEnumerable<CardInstallment> installments);
    Task<CardPurchase?> GetByIdAsync(int id, int userId);
    Task<decimal> SumPendingInstallmentsByCardAsync(int creditCardId, int userId);
    Task<IEnumerable<CardInstallment>> GetInstallmentsByPurchaseAsync(int purchaseId, int userId);
    Task<IReadOnlyList<CardPurchase>> GetDeactivatedOlderThanAsync(DateTime cutoff);
    Task DeleteRangeAsync(IEnumerable<CardPurchase> purchases);
    Task<bool> ExistsInstallmentByCardPurchaseIdAsync(int cardPurchaseId);
    Task<bool> ExistsByCreditCardIdAsync(int creditCardId);
    Task<bool> ExistsByCategoryIdAsync(int categoryId);
    Task<bool> ExistsByCategoryItemIdAsync(int categoryItemId);
}
