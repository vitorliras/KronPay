using System.Text.Json;
using Domain.Entities.Notifications;
using Domain.Enums.Notifications;
using Domain.Interfaces.Notifications;
using Microsoft.Extensions.Logging;

namespace Application.Notifications;

public sealed class NotificationService : INotificationService
{
    private readonly INotificationRepository _notifications;
    private readonly INotificationEmailDispatcher _emailDispatcher;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationRepository notifications,
        INotificationEmailDispatcher emailDispatcher,
        ILogger<NotificationService> logger)
    {
        _notifications = notifications;
        _emailDispatcher = emailDispatcher;
        _logger = logger;
    }

    public async Task ResolveByRelatedEntityAsync(int userId, string relatedEntityType, int relatedEntityId)
    {
        try
        {
            var notifications = await _notifications.GetUnresolvedByRelatedEntityAsync(
                userId, relatedEntityType, relatedEntityId);

            foreach (var notification in notifications)
            {
                notification.Resolve();
                _notifications.Update(notification);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "Falha ao resolver notificações da entidade {RelatedEntityType}/{RelatedEntityId} do usuário {UserId}.",
                relatedEntityType, relatedEntityId, userId);
        }
    }

    public async Task ResolveByTypeAsync(int userId, NotificationType type)
    {
        try
        {
            var notifications = await _notifications.GetUnresolvedByTypeAsync(userId, type);

            foreach (var notification in notifications)
            {
                notification.Resolve();
                _notifications.Update(notification);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Falha ao resolver notificações do tipo {Type} do usuário {UserId}.", type, userId);
        }
    }

    public async Task CreateInstantAsync(
        int userId,
        NotificationType type,
        NotificationCriticality criticality,
        string messageKey,
        IReadOnlyDictionary<string, string>? payload,
        string? relatedEntityType,
        int? relatedEntityId)
    {
        try
        {
            var notification = new Notification(
                userId,
                type,
                criticality,
                messageKey,
                payload is null ? null : JsonSerializer.Serialize(payload),
                relatedEntityType,
                relatedEntityId);

            if (!await _notifications.AddAsync(notification))
                return;

            await _emailDispatcher.DispatchAsync(notification, userId);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Falha ao criar notificação instantânea do tipo {Type} para o usuário {UserId}.", type, userId);
        }
    }
}
