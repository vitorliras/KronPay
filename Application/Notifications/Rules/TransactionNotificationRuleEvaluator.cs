using Domain.Enums.Notifications;
using Domain.Interfaces.Transactions;
using Shared.Localization;

namespace Application.Notifications.Rules;

public sealed class TransactionNotificationRuleEvaluator : ITransactionNotificationRuleEvaluator
{
    private readonly ITransactionRepository _transactions;

    public TransactionNotificationRuleEvaluator(ITransactionRepository transactions)
    {
        _transactions = transactions;
    }

    public async Task<IReadOnlyList<NotificationCandidate>> EvaluateAsync(int userId, DateTime today)
    {
        var transactions = await _transactions.GetOverdueOrDueSoonAsync(userId, today);
        var candidates = new List<NotificationCandidate>();

        foreach (var transaction in transactions)
        {
            var args = new Dictionary<string, string>
            {
                ["description"] = transaction.Description,
                ["amount"] = transaction.Amount.ToString("F2"),
                ["dueDate"] = transaction.TransactionDate.ToString("dd/MM/yyyy")
            };

            NotificationCandidate candidate;

            if (transaction.TransactionDate.Date < today)
            {
                candidate = new NotificationCandidate(
                    NotificationType.TransactionOverdue,
                    NotificationCriticality.Critical,
                    MessageKeys.NotificationTransactionOverdue,
                    args, "Transaction", transaction.Id);
            }
            else if (transaction.TransactionDate.Date == today)
            {
                candidate = new NotificationCandidate(
                    NotificationType.TransactionDueToday,
                    NotificationCriticality.Important,
                    MessageKeys.NotificationTransactionDueToday,
                    args, "Transaction", transaction.Id);
            }
            else
            {
                candidate = new NotificationCandidate(
                    NotificationType.TransactionDueTomorrow,
                    NotificationCriticality.Informative,
                    MessageKeys.NotificationTransactionDueTomorrow,
                    args, "Transaction", transaction.Id);
            }

            candidates.Add(candidate);
        }

        return candidates;
    }
}
