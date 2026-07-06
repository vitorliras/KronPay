namespace Application.DTOs.Goals;

public sealed record GoalsOverviewResponse(
    IEnumerable<FinancialGoalResponse> FinancialGoals,
    IEnumerable<CategoryBudgetGoalResponse> CategoryBudgetGoals);
