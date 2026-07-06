using Domain.Entities.Goals;
using Domain.Enums.Goals;
using Domain.Interfaces.Goals;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Goals;

public sealed class FinancialGoalRepository : IFinancialGoalRepository
{
    private readonly AppDbContext _context;

    public FinancialGoalRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<FinancialGoal?> GetByIdAsync(int id, int userId)
    {
        return await _context.FinancialGoals
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
    }

    public async Task<IEnumerable<FinancialGoal>> GetActiveAsync(int userId)
    {
        return await _context.FinancialGoals
            .Where(x => x.UserId == userId && x.Status == FinancialGoalStatus.Active)
            .ToListAsync();
    }

    public async Task<IEnumerable<FinancialGoal>> GetHistoryAsync(int userId, string? search)
    {
        var query = _context.FinancialGoals
            .Where(x => x.UserId == userId && x.Status != FinancialGoalStatus.Active);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => x.Description.Contains(search));

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> AddAsync(FinancialGoal goal)
    {
        var result = await _context.FinancialGoals.AddAsync(goal);
        return result.State == EntityState.Added;
    }

    public bool Update(FinancialGoal goal)
    {
        var result = _context.FinancialGoals.Update(goal);
        return result.State == EntityState.Modified;
    }
}
