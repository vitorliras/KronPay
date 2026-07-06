using Domain.Exceptions;
using Shared.Localization;

namespace Domain.Entities.Notifications;

public sealed class NotificationPreference
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public bool EmailOnCritical { get; private set; }
    public bool EmailOnImportant { get; private set; }
    public bool EmailOnInformative { get; private set; }

    protected NotificationPreference() { }

    public NotificationPreference(
        int userId,
        bool emailOnCritical = true,
        bool emailOnImportant = true,
        bool emailOnInformative = false)
    {
        if (userId <= 0)
            throw new DomainException(MessageKeys.InvaldUser);

        UserId = userId;
        EmailOnCritical = emailOnCritical;
        EmailOnImportant = emailOnImportant;
        EmailOnInformative = emailOnInformative;
    }

    public void UpdatePreferences(bool emailOnCritical, bool emailOnImportant, bool emailOnInformative)
    {
        EmailOnCritical = emailOnCritical;
        EmailOnImportant = emailOnImportant;
        EmailOnInformative = emailOnInformative;
    }
}
