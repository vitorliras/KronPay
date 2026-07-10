using Application.Services.Gamification;
using Domain.Enums.Gamification;
using Domain.Interfaces.Gamification;
using Domain.Interfaces.Notifications;
using Domain.Services.Gamification;

namespace Application.Services.Gamification.Evaluators;

public sealed class NotificationMissionEvaluator : IMissionEvaluator
{
    private const int QuickResolveHours = 24;
    private const int IgnoredDays = 3;
    private const int RecentResolutionLookbackDays = 2;
    private const string NoPendingAlertsKey = "HasZeroCriticalAlerts";
    private const string QuickResolveCountKey = "NotificationQuickResolveCount";

    private readonly INotificationRepository _notifications;
    private readonly IConsistencyCounterRepository _counters;

    public NotificationMissionEvaluator(INotificationRepository notifications, IConsistencyCounterRepository counters)
    {
        _notifications = notifications;
        _counters = counters;
    }

    public async Task<IReadOnlyList<MissionEvaluationResult>> EvaluateAsync(int userId, DateTime asOf)
    {
        var archived = await _notifications.GetArchivedByUserAsync(userId);
        var quickResolved = archived
            .Where(n => n.IsResolved && n.ResolvedAt.HasValue
                && n.ResolvedAt.Value >= asOf.AddDays(-RecentResolutionLookbackDays)
                && (n.ResolvedAt.Value - n.CreatedAt).TotalHours <= QuickResolveHours)
            .ToList();

        foreach (var notification in quickResolved)
        {
            var isFirstTime = await ConsistencyCounterUpdater.MarkFirstOccurrenceAsync(
                _counters, userId, $"NotificationQuickResolveCounted:NotificationId={notification.Id}");

            if (isFirstTime)
                await ConsistencyCounterUpdater.IncrementOnlyAsync(_counters, userId, QuickResolveCountKey);
        }

        var unresolvedCritical = await _notifications.GetUnresolvedCriticalByUserAsync(userId);
        var hasIgnoredCritical = unresolvedCritical.Any(n => (asOf - n.CreatedAt).TotalDays >= IgnoredDays);

        if (unresolvedCritical.Count == 0)
            await ConsistencyCounterUpdater.MarkOnceAsync(_counters, userId, NoPendingAlertsKey);

        return new List<MissionEvaluationResult>
        {
            new(MissionEventType.NotificationResolvedQuickly, null, quickResolved.Count > 0),
            new(MissionEventType.NotificationCriticalIgnored, null, hasIgnoredCritical)
        };
    }
}
