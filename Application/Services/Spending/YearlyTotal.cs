namespace Application.Services.Spending;

public sealed record YearlyTotal(int Year, decimal Expenses, decimal Income, decimal Balance);
