namespace Application.DTOs.AiChat;

public sealed record GoalContributionDraftResponse(
    int GoalId,
    string GoalDescription,
    decimal Amount);
