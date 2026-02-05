using Domain.Entities.Configuration;

namespace Domain.Interfaces;

public interface IPaymentMethodRepository
{
    Task<bool> AddAsync(PaymentMethod paymentMethod);
    bool Update(PaymentMethod paymentMethod);
    Task<PaymentMethod?> GetByIdAsync(int id, int userId);
    Task<PaymentMethod?> GetByDescriptionAsync(string description, int userId);
    Task<IEnumerable<PaymentMethod>> GetAllAsync(int userId);
}
