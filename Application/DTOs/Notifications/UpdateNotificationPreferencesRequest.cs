namespace Application.DTOs.Notifications;

public sealed record UpdateNotificationPreferencesRequest(
    bool EmailOnCritical,
    bool EmailOnImportant,
    bool EmailOnInformative
);
