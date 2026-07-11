using Domain.Entities.Configuration;

namespace Domain.Interfaces;

public interface ICategoryRepository
{
    Task<bool> AddAsync(Category category);
    bool Update(Category category);
    Task<Category?> GetByIdAsync(int id, int userId);
    Task<Category?> GetByDescriptionAsync(string description, int userId);
    Task<IEnumerable<Category>> GetAllAsync(int userId);
    Task<Category?> GetCardInvoiceCategoryAsync(int userId);
    Task<IReadOnlyList<Category>> GetDeactivatedOlderThanAsync(DateTime cutoff);
    Task DeleteRangeAsync(IEnumerable<Category> categories);
}
