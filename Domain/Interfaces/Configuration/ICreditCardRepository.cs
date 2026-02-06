using Domain.Entities;
using Domain.Entities.Configuration;

namespace Domain.Interfaces;

public interface ICreditCardRepository
{
    Task<bool> AddAsync(CreditCard creditCard);
    bool Update(CreditCard creditCard);
    Task<CreditCard?> GetByIdAsync(int id, int userId);
    Task<CreditCard?> GetByDescriptionAsync(string description, int userId);
    Task<IEnumerable<CreditCard>> GetAllAsync(int userId);
}
