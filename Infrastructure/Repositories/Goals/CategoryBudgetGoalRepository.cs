using Domain.Entities.Goals;
using Domain.Interfaces.Goals;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Goals;

public sealed class CategoryBudgetGoalRepository : ICategoryBudgetGoalRepository
{
    private readonly AppDbContext _context;

    public CategoryBudgetGoalRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CategoryBudgetGoal?> GetByIdAsync(int id, int userId)
    {
        return await _context.CategoryBudgetGoals
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
    }

    public async Task<CategoryBudgetGoal?> GetByCategoryIdAsync(int categoryId, int userId)
    {
        return await _context.CategoryBudgetGoals
            .FirstOrDefaultAsync(x => x.CategoryId == categoryId && x.UserId == userId && x.Active);
    }

    public async Task<IEnumerable<CategoryBudgetGoal>> GetActiveAsync(int userId)
    {
        return await _context.CategoryBudgetGoals
            .Where(x => x.UserId == userId && x.Active)
            .ToListAsync();
    }

    public async Task<bool> AddAsync(CategoryBudgetGoal goal)
    {
        var result = await _context.CategoryBudgetGoals.AddAsync(goal);
        return result.State == EntityState.Added;
    }

    public bool Update(CategoryBudgetGoal goal)
    {
        var result = _context.CategoryBudgetGoals.Update(goal);
        return result.State == EntityState.Modified;
    }

    public async Task<IReadOnlyList<CategoryBudgetGoal>> GetDeactivatedOlderThanAsync(DateTime cutoff)
    {
        return await _context.CategoryBudgetGoals
            .Where(x => !x.Active && x.DeactivatedAt != null && x.DeactivatedAt < cutoff)
            .ToListAsync();
    }

    public Task DeleteRangeAsync(IEnumerable<CategoryBudgetGoal> goals)
    {
        _context.CategoryBudgetGoals.RemoveRange(goals);
        return Task.CompletedTask;
    }
}
