namespace Domain.Enums.Notifications;

public enum NotificationType
{
    TransactionOverdue,
    TransactionDueToday,
    TransactionDueTomorrow,
    CardInvoiceOverdue,
    CardInvoiceDueReminder,
    CardInvoiceClosingReminder,
    CategoryBudgetGoalExceeded,
    CategoryBudgetGoalNearLimit,
    FinancialGoalAtRisk,
    FinancialGoalContributionOverdue,
    FinancialGoalNearCompletionReminder,
    FinancialGoalCompleted,
    FinancialGoalContributionReminder,
    ProjectedSpendingPaceExceedsBudget,
    ProjectedNegativeBalance,
    CategorySpendingAboveAverage,
    RecurringExpenseIncreased,
    MonthlySavingsSummary,
    SignificantSpendingIncrease,
    NetWorthChange,
    CategoryWithoutBudgetGoalHighSpending,
    NoTransactionLoggedRecently
}
