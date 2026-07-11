using Domain.Entities.Goals;

namespace Domain.Interfaces.Goals;

public interface ICategoryBudgetGoalRepository
{
    Task<CategoryBudgetGoal?> GetByIdAsync(int id, int userId);
    Task<CategoryBudgetGoal?> GetByCategoryIdAsync(int categoryId, int userId);
    Task<IEnumerable<CategoryBudgetGoal>> GetActiveAsync(int userId);
    Task<bool> AddAsync(CategoryBudgetGoal goal);
    bool Update(CategoryBudgetGoal goal);
    Task<IReadOnlyList<CategoryBudgetGoal>> GetDeactivatedOlderThanAsync(DateTime cutoff);
    Task DeleteRangeAsync(IEnumerable<CategoryBudgetGoal> goals);
}
