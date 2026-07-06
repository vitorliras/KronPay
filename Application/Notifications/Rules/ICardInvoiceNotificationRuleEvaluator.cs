namespace Application.Notifications.Rules;

public interface ICardInvoiceNotificationRuleEvaluator
{
    Task<IReadOnlyList<NotificationCandidate>> EvaluateAsync(int userId, DateTime today);
}
