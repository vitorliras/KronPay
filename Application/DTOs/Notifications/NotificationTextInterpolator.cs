namespace Application.DTOs.Notifications;

public static class NotificationTextInterpolator
{
    public static string Interpolate(string template, IReadOnlyDictionary<string, string> args) =>
        args.Aggregate(template, (text, pair) => text.Replace($"{{{pair.Key}}}", pair.Value));
}
