namespace Application.DTOs.AiChat;

public sealed record CategoryBudgetGoalDraftResponse(
    int CategoryId,
    decimal MonthlyLimit,
    int Priority);
