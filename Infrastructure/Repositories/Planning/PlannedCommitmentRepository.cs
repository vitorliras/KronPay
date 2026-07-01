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

    // Rastreado (sem AsNoTracking): usado para atualização/desativação persistir.
    public async Task<PlannedCommitment?> GetByIdAsync(int id, int userId)
        => await _context.PlannedCommitments
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

    public async Task<IEnumerable<PlannedCommitment>> GetByUserAsync(int userId)
        => await _context.PlannedCommitments
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.Active)
            .OrderBy(x => x.StartDate)
            .ToListAsync();
}
