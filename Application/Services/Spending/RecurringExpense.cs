namespace Application.Services.Spending;

public sealed record RecurringExpense(
    string Description,
    int MonthsCount,
    decimal AverageAmount,
    DateTime LastDate,
    int? CategoryId);
