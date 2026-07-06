using Domain.Enums.Notifications;
using Domain.Exceptions;
using Shared.Localization;

namespace Domain.Entities.Notifications;

public sealed class Notification
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public NotificationType Type { get; private set; }
    public NotificationCriticality Criticality { get; private set; }
    public string MessageKey { get; private set; } = default!;
    public string? PayloadJson { get; private set; }
    public string? RelatedEntityType { get; private set; }
    public int? RelatedEntityId { get; private set; }
    public bool IsRead { get; private set; }
    public DateTime? ReadAt { get; private set; }
    public bool IsResolved { get; private set; }
    public DateTime? ResolvedAt { get; private set; }
    public bool IsArchived { get; private set; }
    public DateTime? ArchivedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    protected Notification() { }

    public Notification(
        int userId,
        NotificationType type,
        NotificationCriticality criticality,
        string messageKey,
        string? payloadJson = null,
        string? relatedEntityType = null,
        int? relatedEntityId = null)
    {
        if (userId <= 0)
            throw new DomainException(MessageKeys.InvaldUser);

        if (string.IsNullOrWhiteSpace(messageKey))
            throw new DomainException(MessageKeys.InvalidNotificationMessageKey);

        UserId = userId;
        Type = type;
        Criticality = criticality;
        MessageKey = messageKey;
        PayloadJson = payloadJson;
        RelatedEntityType = relatedEntityType;
        RelatedEntityId = relatedEntityId;
        IsRead = false;
        IsResolved = false;
        IsArchived = false;
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsRead()
    {
        IsRead = true;
        ReadAt = DateTime.UtcNow;
    }

    public void Resolve()
    {
        IsResolved = true;
        ResolvedAt = DateTime.UtcNow;
        IsArchived = true;
        ArchivedAt = DateTime.UtcNow;
    }

    public void Archive()
    {
        IsArchived = true;
        ArchivedAt = DateTime.UtcNow;
    }
}
