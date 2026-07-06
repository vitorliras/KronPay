namespace Application.DTOs.Goals;

public sealed record RegisterContributionRequest(int GoalId, decimal Amount);
