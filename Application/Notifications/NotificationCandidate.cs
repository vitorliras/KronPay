using Domain.Enums.Notifications;

namespace Application.Notifications;

public sealed record NotificationCandidate(
    NotificationType Type,
    NotificationCriticality Criticality,
    string MessageKey,
    IReadOnlyDictionary<string, string> Args,
    string? RelatedEntityType,
    int? RelatedEntityId);
