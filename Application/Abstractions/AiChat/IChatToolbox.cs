using System.ComponentModel;

namespace Application.Abstractions.AiChat;

public interface IChatToolbox
{
    [ChatTool]
    [Description("Total expenses grouped by category for a date range. Includes credit card purchases counted by invoice month and excludes credit card invoice payments. Dates are yyyy-MM-dd.")]
    Task<ChatToolResult> GetSpendingByCategory(
        [Description("Start date, yyyy-MM-dd")] string startDate,
        [Description("End date, yyyy-MM-dd")] string endDate);

    [ChatTool]
    [Description("Monthly totals of expenses, income and balance for the last N months (1 to 12), including the current month.")]
    Task<ChatToolResult> GetMonthlyTotals(
        [Description("Number of months, 1 to 12")] int months);

    [ChatTool]
    [Description("Search the user's transactions and card purchases. Returns at most 30 rows, newest first. All filters are optional.")]
    Task<ChatToolResult> SearchTransactions(
        [Description("Text contained in the description")] string? text = null,
        [Description("Category name")] string? category = null,
        [Description("Start date, yyyy-MM-dd. Defaults to 90 days ago")] string? startDate = null,
        [Description("End date, yyyy-MM-dd. Defaults to today")] string? endDate = null,
        [Description("expense, income or all. Defaults to expense")] string? kind = null,
        [Description("Maximum rows, 1 to 30")] int limit = 20);

    [ChatTool]
    [Description("Credit cards with limit, used amount, available amount and the next unpaid invoices.")]
    Task<ChatToolResult> GetCardsOverview();

    [ChatTool]
    [Description("Active financial goals with progress and active planned recurring commitments.")]
    Task<ChatToolResult> GetGoalsAndCommitments();

    [ChatTool]
    [Description("Expenses that repeat every month (subscriptions, bills) detected over the last 6 months.")]
    Task<ChatToolResult> GetRecurringExpenses();

    [ChatTool]
    [Description("Offer the user a button to open a screen. Valid screens: dashboard, transactions, reports, planning, cards, invoices, goals, config.")]
    Task<ChatToolResult> OpenScreen(
        [Description("Screen id")] string screen);

    [ChatTool]
    [Description("Offer the user a button to start the guided tour of a screen. Valid screens: dashboard, transactions, reports, planning, cards, goals, config.")]
    Task<ChatToolResult> StartTour(
        [Description("Screen id")] string screen);

    [ChatTool]
    [Description("Open the transaction form for the user to review and save. REQUIRED for each item: description and amount. If anything required is missing, nothing is opened and you must ask the user. Nothing is saved by this tool.")]
    Task<ChatToolResult> PrepareTransactions(
        [Description("The transactions the user described, with only what they actually said")] IReadOnlyList<TransactionToolItem>? items = null);

    [ChatTool]
    [Description("Open the credit card purchase form for the user to review and save. REQUIRED: card and, for each item, description and total amount. If anything required is missing, nothing is opened and you must ask the user. Nothing is saved by this tool.")]
    Task<ChatToolResult> PrepareCardPurchases(
        [Description("Credit card name as the user said it")] string? cardName = null,
        [Description("The purchases the user described, with only what they actually said")] IReadOnlyList<CardPurchaseToolItem>? items = null);

    [ChatTool]
    [Description("Open the form to create a category. REQUIRED: description and type (expense, income or investment). If missing, nothing is opened and you must ask the user. Nothing is saved by this tool.")]
    Task<ChatToolResult> PrepareCategory(
        [Description("Category name")] string? description = null,
        [Description("expense, income or investment")] string? type = null);

    [ChatTool]
    [Description("Open the form to create a subcategory. REQUIRED: category and description. If missing or ambiguous, nothing is opened and you must ask the user. Nothing is saved by this tool.")]
    Task<ChatToolResult> PrepareSubcategory(
        [Description("Name of the parent category")] string? categoryName = null,
        [Description("Subcategory name")] string? description = null);

    [ChatTool]
    [Description("Open the form to create a payment method. REQUIRED: description. If missing, nothing is opened and you must ask the user. Nothing is saved by this tool.")]
    Task<ChatToolResult> PreparePaymentMethod(
        [Description("Payment method name, for example Pix")] string? description = null);

    [ChatTool]
    [Description("Open the form to create a financial goal (objetivo). REQUIRED: description, target amount and target date. If missing, nothing is opened and you must ask the user. Nothing is saved by this tool.")]
    Task<ChatToolResult> PrepareFinancialGoal(
        [Description("What the goal is for")] string? description = null,
        [Description("Target amount in BRL")] decimal? targetAmount = null,
        [Description("Target date yyyy-MM-dd, in the future")] string? targetDate = null,
        [Description("low, medium or high. Omit for medium")] string? priority = null);

    [ChatTool]
    [Description("Open the form to create a monthly spending limit for a category (budget goal). REQUIRED: category and monthly limit. If missing or ambiguous, nothing is opened and you must ask the user. Nothing is saved by this tool.")]
    Task<ChatToolResult> PrepareCategoryBudgetGoal(
        [Description("Expense category name")] string? categoryName = null,
        [Description("Monthly limit in BRL")] decimal? monthlyLimit = null,
        [Description("low, medium or high. Omit for medium")] string? priority = null);

    [ChatTool]
    [Description("Open the form to register a contribution (aporte) into an active financial goal. REQUIRED: goal and amount. If missing or ambiguous, nothing is opened and you must ask the user. Nothing is saved by this tool.")]
    Task<ChatToolResult> PrepareGoalContribution(
        [Description("Name of the financial goal as the user said it")] string? goalName = null,
        [Description("Amount contributed in BRL")] decimal? amount = null);

    [ChatTool]
    [Description("Open the form to create a planned recurring commitment (rent, salary, subscription). REQUIRED: description, amount and direction (expense or income). If missing, nothing is opened and you must ask the user. Nothing is saved by this tool.")]
    Task<ChatToolResult> PreparePlannedCommitment(
        [Description("What the commitment is, for example Aluguel")] string? description = null,
        [Description("Amount in BRL")] decimal? amount = null,
        [Description("expense or income")] string? direction = null,
        [Description("once, monthly, weekly or annual. Omit for monthly")] string? periodicity = null,
        [Description("Start date yyyy-MM-dd. Monthly and yearly commitments repeat on the day of this date, so for every day N use the next date whose day is N. Omit for today")] string? startDate = null,
        [Description("End date yyyy-MM-dd. Omit when it has no end")] string? endDate = null,
        [Description("Category name if the user said it")] string? categoryName = null);

    [ChatTool]
    [Description("Prepare the payment of a credit card invoice. Nothing is paid by this tool: the chat shows the invoice and asks the user which payment method was used, and the payment is only registered when the user answers.")]
    Task<ChatToolResult> PrepareInvoicePayment(
        [Description("Credit card name as the user said it")] string cardName,
        [Description("Invoice reference month 1-12. Omit to use the oldest unpaid invoice")] int? month = null,
        [Description("Invoice reference year. Omit to use the oldest unpaid invoice")] int? year = null,
        [Description("Payment method name if the user already said it")] string? paymentMethodName = null);
}
