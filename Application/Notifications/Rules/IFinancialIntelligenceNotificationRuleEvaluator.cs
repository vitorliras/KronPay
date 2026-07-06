namespace Application.Notifications.Rules;

public interface IFinancialIntelligenceNotificationRuleEvaluator
{
    Task<IReadOnlyList<NotificationCandidate>> EvaluateAsync(int userId, DateTime today);
}
