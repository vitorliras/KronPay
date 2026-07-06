using Domain.Entities.Notifications;
using Domain.Enums.Notifications;
using Domain.Interfaces.Notifications;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Notifications;

public sealed class NotificationRepository : INotificationRepository
{
    private readonly AppDbContext _context;

    public NotificationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> AddAsync(Notification notification)
    {
        var result = await _context.Notifications.AddAsync(notification);
        return result.State == EntityState.Added;
    }

    public bool Update(Notification notification)
    {
        var result = _context.Notifications.Update(notification);
        return result.State == EntityState.Modified;
    }

    public async Task<Notification?> GetByIdAsync(int id, int userId)
    {
        return await _context.Notifications
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
    }

    public async Task<IReadOnlyList<Notification>> GetActiveByUserAsync(int userId)
    {
        return await _context.Notifications
            .AsNoTracking()
            .Where(x => x.UserId == userId && !x.IsArchived)
            .OrderBy(x => x.Criticality)
            .ThenByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Notification>> GetArchivedByUserAsync(int userId)
    {
        return await _context.Notifications
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.IsArchived)
            .OrderByDescending(x => x.ArchivedAt)
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        return await _context.Notifications
            .AsNoTracking()
            .CountAsync(x => x.UserId == userId && !x.IsArchived && !x.IsRead);
    }

    public async Task<IReadOnlyList<Notification>> GetUnresolvedCriticalByUserAsync(int userId)
    {
        return await _context.Notifications
            .Where(x => x.UserId == userId && !x.IsResolved && x.Criticality == NotificationCriticality.Critical)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<Notification?> GetExistingUnresolvedAsync(
        int userId, NotificationType type, string? relatedEntityType, int? relatedEntityId)
    {
        return await _context.Notifications
            .FirstOrDefaultAsync(x =>
                x.UserId == userId &&
                x.Type == type &&
                !x.IsResolved &&
                x.RelatedEntityType == relatedEntityType &&
                x.RelatedEntityId == relatedEntityId);
    }

    public async Task<IReadOnlyList<Notification>> GetUnresolvedByTypeAsync(int userId, NotificationType type)
    {
        return await _context.Notifications
            .Where(x => x.UserId == userId && x.Type == type && !x.IsResolved)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Notification>> GetArchivedOlderThanAsync(DateTime cutoff)
    {
        return await _context.Notifications
            .Where(x => x.IsArchived && x.ArchivedAt != null && x.ArchivedAt < cutoff)
            .ToListAsync();
    }

    public Task DeleteRangeAsync(IEnumerable<Notification> notifications)
    {
        _context.Notifications.RemoveRange(notifications);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<Notification>> GetUnresolvedByRelatedEntityAsync(
        int userId, string relatedEntityType, int relatedEntityId)
    {
        return await _context.Notifications
            .Where(x =>
                x.UserId == userId &&
                !x.IsResolved &&
                x.RelatedEntityType == relatedEntityType &&
                x.RelatedEntityId == relatedEntityId)
            .ToListAsync();
    }
}
