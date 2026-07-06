using Domain.Entities.Notifications;

namespace Domain.Interfaces.Notifications;

public interface INotificationEvaluationRunRepository
{
    Task<NotificationEvaluationRun?> GetAsync();
    Task<bool> AddAsync(NotificationEvaluationRun run);
    bool Update(NotificationEvaluationRun run);
}
