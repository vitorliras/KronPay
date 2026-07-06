namespace Application.Notifications.Rules;

public interface IGoalNotificationRuleEvaluator
{
    Task<IReadOnlyList<NotificationCandidate>> EvaluateAsync(int userId, DateTime today);
}
