using Domain.Enums.Notifications;

namespace Application.Notifications;

public interface INotificationService
{
    Task ResolveByRelatedEntityAsync(int userId, string relatedEntityType, int relatedEntityId);
    Task ResolveByTypeAsync(int userId, NotificationType type);

    Task CreateInstantAsync(
        int userId,
        NotificationType type,
        NotificationCriticality criticality,
        string messageKey,
        IReadOnlyDictionary<string, string>? payload,
        string? relatedEntityType,
        int? relatedEntityId);
}
