using Domain.Enums.Notifications;
using Domain.Interfaces.Transactions;
using Shared.Localization;

namespace Application.Notifications.Rules;

public sealed class DataHygieneNotificationRuleEvaluator : IDataHygieneNotificationRuleEvaluator
{
    private const int InactivityThresholdDays = 7;

    private readonly ITransactionRepository _transactions;

    public DataHygieneNotificationRuleEvaluator(ITransactionRepository transactions)
    {
        _transactions = transactions;
    }

    public async Task<IReadOnlyList<NotificationCandidate>> EvaluateAsync(int userId, DateTime today)
    {
        var lastDate = await _transactions.GetLastTransactionDateAsync(userId);
        if (lastDate is null)
            return Array.Empty<NotificationCandidate>();

        var daysSince = (today - lastDate.Value.Date).Days;
        if (daysSince < InactivityThresholdDays)
            return Array.Empty<NotificationCandidate>();

        var args = new Dictionary<string, string> { ["days"] = daysSince.ToString() };

        return new[]
        {
            new NotificationCandidate(
                NotificationType.NoTransactionLoggedRecently,
                NotificationCriticality.Important,
                MessageKeys.NotificationNoTransactionLoggedRecently,
                args, null, null)
        };
    }
}
