using Domain.Entities.DataRetention;
using Domain.Interfaces.DataRetention;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.DataRetention;

public sealed class DataRetentionPurgeRunRepository : IDataRetentionPurgeRunRepository
{
    private readonly AppDbContext _context;

    public DataRetentionPurgeRunRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DataRetentionPurgeRun?> GetAsync()
    {
        return await _context.DataRetentionPurgeRuns.FirstOrDefaultAsync();
    }

    public async Task<bool> AddAsync(DataRetentionPurgeRun run)
    {
        var result = await _context.DataRetentionPurgeRuns.AddAsync(run);
        return result.State == EntityState.Added;
    }

    public bool Update(DataRetentionPurgeRun run)
    {
        var result = _context.DataRetentionPurgeRuns.Update(run);
        return result.State == EntityState.Modified;
    }
}
