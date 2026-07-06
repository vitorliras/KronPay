namespace Application.DTOs.Goals;

public sealed record SimulateExtraContributionRequest(int GoalId, decimal ExtraMonthlyAmount);
