using Application.DTOs.Assistant;
using Application.DTOs.Notifications;
using Application.Notifications;
using Domain.Entities.Notifications;
using Domain.Interfaces;
using Domain.Interfaces.Notifications;
using Microsoft.Extensions.Localization;
using Shared.Localization;
using Shared.Resources;

namespace Application.Services.Assistant;

public sealed class NotificationAssistantResolver
{
    private const int MaxVisibleNotifications = 5;
    private const int NotificationTextMaxLength = 60;

    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStringLocalizer<Messages> _localizer;

    public NotificationAssistantResolver(
        INotificationRepository notificationRepository,
        IUnitOfWork unitOfWork,
        IStringLocalizer<Messages> localizer)
    {
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
        _localizer = localizer;
    }

    public async Task<int> GetUnreadCountAsync(int userId) => (await GetUnreadAsync(userId)).Count;

    public async Task<IReadOnlyList<AssistantOptionResponse>> BuildListOptionsAsync(int userId)
    {
        var unread = await GetUnreadAsync(userId);

        var options = unread
            .Take(MaxVisibleNotifications)
            .Select(n => new AssistantOptionResponse(
                $"notification:{n.Id}",
                MessageKeys.AssistantDynamicLabel,
                new[] { AssistantTextHelper.Truncate(RenderText(n), NotificationTextMaxLength) }))
            .ToList();

        if (unread.Count > 0)
            options.Add(new AssistantOptionResponse("notifications:mark-all", MessageKeys.AssistantOptionMarkAllAsRead, Array.Empty<string>()));

        return options;
    }

    public async Task<bool> MarkAsReadAsync(int notificationId, int userId)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId, userId);
        if (notification is null)
            return false;

        MarkAsReadWithArchiveRule(notification);

        if (!_notificationRepository.Update(notification))
            return false;

        return await _unitOfWork.CommitAsync();
    }

    public async Task<int> MarkAllAsReadAsync(int userId)
    {
        var unread = await GetUnreadAsync(userId);
        if (unread.Count == 0)
            return 0;

        foreach (var notification in unread)
        {
            MarkAsReadWithArchiveRule(notification);
            _notificationRepository.Update(notification);
        }

        return await _unitOfWork.CommitAsync() ? unread.Count : 0;
    }

    private static void MarkAsReadWithArchiveRule(Notification notification)
    {
        notification.MarkAsRead();

        if (NotificationTypeCatalog.IsEventBased(notification.Type))
            notification.Archive();
    }

    private async Task<IReadOnlyList<Notification>> GetUnreadAsync(int userId)
    {
        var active = await _notificationRepository.GetActiveByUserAsync(userId);
        return active
            .Where(n => !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .ToList();
    }

    private string RenderText(Notification notification)
    {
        var template = _localizer[notification.MessageKey].Value;
        var args = NotificationResponseMapper.ToResponse(notification).Args;
        return NotificationTextInterpolator.Interpolate(template, args);
    }
}
