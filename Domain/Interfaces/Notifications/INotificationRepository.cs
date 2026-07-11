using Domain.Entities.Notifications;
using Domain.Enums.Notifications;

namespace Domain.Interfaces.Notifications;

public interface INotificationRepository
{
    Task<bool> AddAsync(Notification notification);
    bool Update(Notification notification);
    Task<Notification?> GetByIdAsync(int id, int userId);
    Task<IReadOnlyList<Notification>> GetActiveByUserAsync(int userId);
    Task<IReadOnlyList<Notification>> GetArchivedByUserAsync(int userId);
    Task<int> GetUnreadCountAsync(int userId);
    Task<IReadOnlyList<Notification>> GetUnresolvedCriticalByUserAsync(int userId);

    Task<Notification?> GetExistingUnresolvedAsync(
        int userId, NotificationType type, string? relatedEntityType, int? relatedEntityId);
    Task<IReadOnlyList<Notification>> GetUnresolvedByTypeAsync(int userId, NotificationType type);
    Task<IReadOnlyList<Notification>> GetArchivedOlderThanAsync(DateTime cutoff);
    Task DeleteRangeAsync(IEnumerable<Notification> notifications);

    Task<IReadOnlyList<Notification>> GetUnresolvedByRelatedEntityAsync(
        int userId, string relatedEntityType, int relatedEntityId);
}
