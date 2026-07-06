namespace Application.DTOs.Goals;

public sealed record GoalHistoryResponse(IReadOnlyList<FinancialGoalResponse> Goals);
