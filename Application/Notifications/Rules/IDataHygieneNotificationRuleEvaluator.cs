namespace Application.Notifications.Rules;

public interface IDataHygieneNotificationRuleEvaluator
{
    Task<IReadOnlyList<NotificationCandidate>> EvaluateAsync(int userId, DateTime today);
}
