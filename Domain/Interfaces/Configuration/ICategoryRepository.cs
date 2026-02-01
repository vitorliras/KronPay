using Domain.Entities.Configuration;

namespace Domain.Interfaces;

public interface ICategoryRepository
{
    Task<bool> AddAsync(Category category);
    bool UpdateAsync(Category category);
    Task<Category?> GetByIdAsync(int id, int userId);
    Task<Category?> GetByDescriptionAsync(string description, int userId);
    Task<IEnumerable<Category>> GetAllAsync(int userId);
}
