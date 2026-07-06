namespace Application.DTOs.Goals;

public sealed record RetryFinancialGoalRequest(int GoalId, DateTime NewTargetDate);
