using Domain.Entities.Gamification;
using Domain.Enums.Gamification;
using Domain.Interfaces.Gamification;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Gamification;

public sealed class MissionStateSnapshotRepository : IMissionStateSnapshotRepository
{
    private readonly AppDbContext _context;

    public MissionStateSnapshotRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<MissionStateSnapshot?> GetAsync(int userId, MissionEventType type, int? relatedEntityId)
    {
        return await _context.MissionStateSnapshots
            .FirstOrDefaultAsync(x =>
                x.UserId == userId &&
                x.Type == type &&
                x.RelatedEntityId == relatedEntityId);
    }

    public async Task<IEnumerable<MissionStateSnapshot>> GetAllByUserAsync(int userId)
    {
        return await _context.MissionStateSnapshots
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .ToListAsync();
    }

    public async Task<bool> AddAsync(MissionStateSnapshot snapshot)
    {
        var result = _context.MissionStateSnapshots.Add(snapshot);
        return result.State == EntityState.Added;
    }

    public bool Update(MissionStateSnapshot snapshot)
    {
        var result = _context.MissionStateSnapshots.Update(snapshot);
        return result.State == EntityState.Modified;
    }
}
