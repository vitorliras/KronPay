using Domain.Entities.Gamification;
using Domain.Interfaces.Gamification;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Gamification;

public sealed class ConsistencyCounterRepository : IConsistencyCounterRepository
{
    private readonly AppDbContext _context;

    public ConsistencyCounterRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ConsistencyCounter?> GetAsync(int userId, string counterKey)
    {
        return await _context.ConsistencyCounters
            .FirstOrDefaultAsync(x => x.UserId == userId && x.CounterKey == counterKey);
    }

    public async Task<IEnumerable<ConsistencyCounter>> GetAllByUserAsync(int userId)
    {
        var persisted = await _context.ConsistencyCounters
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .ToListAsync();

        var merged = persisted.ToDictionary(x => x.CounterKey);

        foreach (var tracked in _context.ConsistencyCounters.Local.Where(x => x.UserId == userId))
            merged[tracked.CounterKey] = tracked;

        return merged.Values;
    }

    public async Task<bool> AddAsync(ConsistencyCounter counter)
    {
        var result = _context.ConsistencyCounters.Add(counter);
        return result.State == EntityState.Added;
    }

    public bool Update(ConsistencyCounter counter)
    {
        var result = _context.ConsistencyCounters.Update(counter);
        return result.State == EntityState.Modified;
    }
}
