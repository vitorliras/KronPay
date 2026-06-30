using Domain.Entities.Card;

namespace Domain.Interfaces.Card;

public interface ICardPurchaseRepository
{
    Task<bool> AddAsync(CardPurchase purchase);
    Task<bool> AddInstallmentsAsync(IEnumerable<CardInstallment> installments);
    Task<CardPurchase?> GetByIdAsync(int id, int userId);
    Task<decimal> SumPendingInstallmentsByCardAsync(int creditCardId, int userId);
    Task<IEnumerable<CardInstallment>> GetInstallmentsByPurchaseAsync(int purchaseId, int userId);
}
