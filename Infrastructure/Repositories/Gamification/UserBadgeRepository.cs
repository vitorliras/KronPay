using Domain.Entities.Gamification;
using Domain.Enums.Gamification;
using Domain.Interfaces.Gamification;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Gamification;

public sealed class UserBadgeRepository : IUserBadgeRepository
{
    private readonly AppDbContext _context;

    public UserBadgeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> AddAsync(UserBadge badge)
    {
        var result = _context.UserBadges.Add(badge);
        return result.State == EntityState.Added;
    }

    public async Task<IEnumerable<UserBadge>> GetByUserIdAsync(int userId)
    {
        return await _context.UserBadges
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(int userId, BadgeCode code)
    {
        return await _context.UserBadges
            .AsNoTracking()
            .AnyAsync(x => x.UserId == userId && x.Code == code);
    }
}
