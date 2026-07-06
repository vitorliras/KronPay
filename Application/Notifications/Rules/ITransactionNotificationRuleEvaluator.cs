namespace Application.Notifications.Rules;

public interface ITransactionNotificationRuleEvaluator
{
    Task<IReadOnlyList<NotificationCandidate>> EvaluateAsync(int userId, DateTime today);
}
