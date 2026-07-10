using Domain.Entities.Gamification;
using Domain.Interfaces.Gamification;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Gamification;

public sealed class PointLedgerRepository : IPointLedgerRepository
{
    private readonly AppDbContext _context;

    public PointLedgerRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> AddAsync(PointLedgerEntry entry)
    {
        var result = _context.PointLedgerEntries.Add(entry);
        return result.State == EntityState.Added;
    }
}
