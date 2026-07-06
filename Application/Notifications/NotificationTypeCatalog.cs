using Domain.Enums.Notifications;

namespace Application.Notifications;

public static class NotificationTypeCatalog
{
    private static readonly IReadOnlySet<NotificationType> EventBasedTypes = new HashSet<NotificationType>
    {
        NotificationType.FinancialGoalCompleted,
        NotificationType.CategorySpendingAboveAverage,
        NotificationType.RecurringExpenseIncreased,
        NotificationType.MonthlySavingsSummary,
        NotificationType.SignificantSpendingIncrease,
        NotificationType.NetWorthChange,
        NotificationType.CategoryWithoutBudgetGoalHighSpending,
        NotificationType.CardInvoiceClosingReminder
    };

    public static bool IsEventBased(NotificationType type) => EventBasedTypes.Contains(type);
}
