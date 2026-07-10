using Domain.Entities.Gamification;
using Domain.Interfaces.Gamification;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Gamification;

public sealed class UserRankProfileRepository : IUserRankProfileRepository
{
    private readonly AppDbContext _context;

    public UserRankProfileRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> AddAsync(UserRankProfile profile)
    {
        var result = _context.UserRankProfiles.Add(profile);
        return result.State == EntityState.Added;
    }

    public bool Update(UserRankProfile profile)
    {
        if (_context.Entry(profile).State == EntityState.Added)
            return true;

        var result = _context.UserRankProfiles.Update(profile);
        return result.State == EntityState.Modified;
    }

    public async Task<UserRankProfile?> GetByUserIdAsync(int userId)
    {
        var tracked = _context.UserRankProfiles.Local.FirstOrDefault(x => x.UserId == userId);
        if (tracked is not null)
            return tracked;

        return await _context.UserRankProfiles
            .FirstOrDefaultAsync(x => x.UserId == userId);
    }
}
