using System.Text.Json;
using Domain.Entities.Notifications;

namespace Application.DTOs.Notifications;

public static class NotificationResponseMapper
{
    public static NotificationResponse ToResponse(Notification notification)
    {
        var args = string.IsNullOrWhiteSpace(notification.PayloadJson)
            ? new Dictionary<string, string>()
            : JsonSerializer.Deserialize<Dictionary<string, string>>(notification.PayloadJson!)
                ?? new Dictionary<string, string>();

        return new NotificationResponse(
            notification.Id,
            notification.Type.ToString(),
            notification.Criticality.ToString(),
            notification.MessageKey,
            args,
            notification.RelatedEntityType,
            notification.RelatedEntityId,
            notification.IsRead,
            notification.CreatedAt);
    }
}
