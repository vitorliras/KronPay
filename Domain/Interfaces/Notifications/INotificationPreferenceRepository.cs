using Domain.Entities.Notifications;

namespace Domain.Interfaces.Notifications;

public interface INotificationPreferenceRepository
{
    Task<NotificationPreference?> GetByUserIdAsync(int userId);
    Task<bool> AddAsync(NotificationPreference preference);
    bool Update(NotificationPreference preference);
}
