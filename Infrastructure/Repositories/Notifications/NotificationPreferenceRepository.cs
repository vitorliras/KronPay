using Domain.Entities.Notifications;
using Domain.Interfaces.Notifications;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Notifications;

public sealed class NotificationPreferenceRepository : INotificationPreferenceRepository
{
    private readonly AppDbContext _context;

    public NotificationPreferenceRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationPreference?> GetByUserIdAsync(int userId)
    {
        return await _context.NotificationPreferences
            .FirstOrDefaultAsync(x => x.UserId == userId);
    }

    public async Task<bool> AddAsync(NotificationPreference preference)
    {
        var result = await _context.NotificationPreferences.AddAsync(preference);
        return result.State == EntityState.Added;
    }

    public bool Update(NotificationPreference preference)
    {
        var result = _context.NotificationPreferences.Update(preference);
        return result.State == EntityState.Modified;
    }
}
