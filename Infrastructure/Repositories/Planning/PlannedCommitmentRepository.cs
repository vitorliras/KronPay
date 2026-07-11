using Domain.Entities.Planning;
using Domain.Interfaces.Planning;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Planning;

public sealed class PlannedCommitmentRepository : IPlannedCommitmentRepository
{
    private readonly AppDbContext _context;

    public PlannedCommitmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> AddAsync(PlannedCommitment commitment)
    {
        var result = await _context.PlannedCommitments.AddAsync(commitment);
        return result.State == EntityState.Added;
    }

    public bool Update(PlannedCommitment commitment)
    {
        var result = _context.PlannedCommitments.Update(commitment);
        return result.State == EntityState.Modified;
    }

    public async Task<PlannedCommitment?> GetByIdAsync(int id, int userId)
        => await _context.PlannedCommitments
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

    public async Task<IEnumerable<PlannedCommitment>> GetByUserAsync(int userId)
        => await _context.PlannedCommitments
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.Active)
            .OrderBy(x => x.StartDate)
            .ToListAsync();

    public async Task<IReadOnlyList<PlannedCommitment>> GetDeactivatedOlderThanAsync(DateTime cutoff)
    {
        return await _context.PlannedCommitments
            .Where(x => !x.Active && x.DeactivatedAt != null && x.DeactivatedAt < cutoff)
            .ToListAsync();
    }

    public Task DeleteRangeAsync(IEnumerable<PlannedCommitment> commitments)
    {
        _context.PlannedCommitments.RemoveRange(commitments);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsByCategoryIdAsync(int categoryId)
    {
        return await _context.PlannedCommitments
            .AsNoTracking()
            .AnyAsync(x => x.CategoryId == categoryId);
    }
}
