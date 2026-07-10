using Domain.Entities.Gamification;
using Domain.Interfaces.Gamification;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Gamification;

public sealed class GamificationEvaluationRunRepository : IGamificationEvaluationRunRepository
{
    private readonly AppDbContext _context;

    public GamificationEvaluationRunRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<GamificationEvaluationRun?> GetAsync()
    {
        return await _context.GamificationEvaluationRuns
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> AddAsync(GamificationEvaluationRun run)
    {
        var result = await _context.GamificationEvaluationRuns.AddAsync(run);
        return result.State == EntityState.Added;
    }

    public bool Update(GamificationEvaluationRun run)
    {
        var result = _context.GamificationEvaluationRuns.Update(run);
        return result.State == EntityState.Modified;
    }
}
