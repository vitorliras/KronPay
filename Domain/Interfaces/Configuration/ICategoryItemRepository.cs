using Domain.Entities;
using Domain.Entities.Configuration;

namespace Domain.Interfaces;

public interface ICategoryItemRepository
{
    Task<bool> AddAsync(CategoryItem item);
    bool Update(CategoryItem item);
    Task<CategoryItem?> GetByIdAsync(int id, int categoryId);
    Task<CategoryItem?> GetByDescriptionAsync(string description, int categoryId);
    Task<IEnumerable<CategoryItem>> GetAllAsync(int categoryId);
}
