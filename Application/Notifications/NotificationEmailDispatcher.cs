using Application.Abstractions.Email;
using Application.DTOs.Notifications;
using Domain.Entities.Notifications;
using Domain.Enums.Notifications;
using Domain.interfaces;
using Domain.Interfaces.Notifications;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Shared.Localization;
using Shared.Resources;

namespace Application.Notifications;

public sealed class NotificationEmailDispatcher : INotificationEmailDispatcher
{
    private readonly INotificationPreferenceRepository _preferences;
    private readonly IUserRepository _users;
    private readonly IEmailSender _emailSender;
    private readonly IStringLocalizer<Messages> _localizer;
    private readonly ILogger<NotificationEmailDispatcher> _logger;

    public NotificationEmailDispatcher(
        INotificationPreferenceRepository preferences,
        IUserRepository users,
        IEmailSender emailSender,
        IStringLocalizer<Messages> localizer,
        ILogger<NotificationEmailDispatcher> logger)
    {
        _preferences = preferences;
        _users = users;
        _emailSender = emailSender;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task DispatchAsync(Notification notification, int userId)
    {
        try
        {
            var preference = await _preferences.GetByUserIdAsync(userId);
            if (!ShouldSendEmail(notification.Criticality, preference))
                return;

            var user = await _users.GetByIdAsync(userId);
            if (user is null)
                return;

            var args = NotificationResponseMapper.ToResponse(notification).Args;
            var body = InterpolateTemplate(_localizer[notification.MessageKey].Value, args);

            await _emailSender.SendAsync(
                user.Email.Value,
                _localizer[MessageKeys.NotificationEmailSubject],
                body);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Falha ao enviar e-mail de notificação para o usuário {UserId}.", userId);
        }
    }

    private static bool ShouldSendEmail(NotificationCriticality criticality, NotificationPreference? preference)
    {
        if (preference is null)
            return criticality != NotificationCriticality.Informative;

        return criticality switch
        {
            NotificationCriticality.Critical => preference.EmailOnCritical,
            NotificationCriticality.Important => preference.EmailOnImportant,
            _ => preference.EmailOnInformative
        };
    }

    private static string InterpolateTemplate(string template, IReadOnlyDictionary<string, string> args)
    {
        foreach (var (key, value) in args)
            template = template.Replace("{" + key + "}", value);

        return template;
    }
}
