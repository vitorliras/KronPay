using Domain.Entities.AI;
using Domain.Interfaces.AI;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.AI;

public sealed class AiUsageLogRepository : IAiUsageLogRepository
{
    private readonly AppDbContext _context;

    public AiUsageLogRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> AddAsync(AiUsageLog log)
    {
        var result = await _context.AiUsageLogs.AddAsync(log);
        return result.State == EntityState.Added;
    }

    public async Task<IReadOnlyList<AiUsageLog>> GetByPeriodAsync(
        int userId,
        DateTime fromInclusive,
        DateTime toExclusive)
    {
        return await _context.AiUsageLogs
            .AsNoTracking()
            .Where(l => l.UserId == userId
                        && l.CreatedAt >= fromInclusive
                        && l.CreatedAt < toExclusive)
            .ToListAsync();
    }

    public async Task<int> CountByFeatureSinceAsync(int userId, string feature, DateTime sinceInclusive)
    {
        return await _context.AiUsageLogs
            .AsNoTracking()
            .CountAsync(l => l.UserId == userId && l.Feature == feature && l.CreatedAt >= sinceInclusive);
    }
}
