namespace Application.DTOs.Goals;

public sealed record GoalViabilityResponse(
    bool AtRisk,
    IReadOnlyList<GoalStrategyOptionResponse> Strategies);
