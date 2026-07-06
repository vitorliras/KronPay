namespace Application.DTOs.Notifications;

public sealed record NotificationPreferenceResponse(
    bool EmailOnCritical,
    bool EmailOnImportant,
    bool EmailOnInformative
);
