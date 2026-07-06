namespace Application.DTOs.Notifications;

public sealed record NotificationResponse(
    int Id,
    string Type,
    string Criticality,
    string MessageKey,
    IReadOnlyDictionary<string, string> Args,
    string? RelatedEntityType,
    int? RelatedEntityId,
    bool IsRead,
    DateTime CreatedAt
);
