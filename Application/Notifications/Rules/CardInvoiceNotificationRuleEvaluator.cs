using Domain.Enums.Notifications;
using Domain.Interfaces.Card;
using Shared.Localization;

namespace Application.Notifications.Rules;

public sealed class CardInvoiceNotificationRuleEvaluator : ICardInvoiceNotificationRuleEvaluator
{
    private readonly ICardInvoiceRepository _invoices;

    public CardInvoiceNotificationRuleEvaluator(ICardInvoiceRepository invoices)
    {
        _invoices = invoices;
    }

    public async Task<IReadOnlyList<NotificationCandidate>> EvaluateAsync(int userId, DateTime today)
    {
        var invoices = await _invoices.GetByUserAsync(userId);
        var candidates = new List<NotificationCandidate>();
        var tomorrow = today.AddDays(1);

        foreach (var invoice in invoices)
        {
            if (invoice.IsPaid)
                continue;

            var dueArgs = new Dictionary<string, string>
            {
                ["amount"] = invoice.TotalAmount.ToString("F2"),
                ["dueDate"] = invoice.DueDate.ToString("dd/MM/yyyy")
            };

            if (invoice.DueDate.Date < today)
            {
                candidates.Add(new NotificationCandidate(
                    NotificationType.CardInvoiceOverdue,
                    NotificationCriticality.Critical,
                    MessageKeys.NotificationCardInvoiceOverdue,
                    dueArgs, "CardInvoice", invoice.Id));
            }
            else if (invoice.DueDate.Date == today || invoice.DueDate.Date == tomorrow)
            {
                candidates.Add(new NotificationCandidate(
                    NotificationType.CardInvoiceDueReminder,
                    NotificationCriticality.Important,
                    MessageKeys.NotificationCardInvoiceDueReminder,
                    dueArgs, "CardInvoice", invoice.Id));
            }

            if (invoice.Status == "A" && (invoice.ClosingDate.Date == today || invoice.ClosingDate.Date == tomorrow))
            {
                var closingArgs = new Dictionary<string, string>
                {
                    ["closingDate"] = invoice.ClosingDate.ToString("dd/MM/yyyy")
                };

                candidates.Add(new NotificationCandidate(
                    NotificationType.CardInvoiceClosingReminder,
                    NotificationCriticality.Informative,
                    MessageKeys.NotificationCardInvoiceClosingReminder,
                    closingArgs, "CardInvoice", invoice.Id));
            }
        }

        return candidates;
    }
}
