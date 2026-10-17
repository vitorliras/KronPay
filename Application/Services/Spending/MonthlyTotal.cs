namespace Application.Services.Spending;

public sealed record MonthlyTotal(string Month, decimal Expenses, decimal Income, decimal Balance);
