namespace Application.DTOs.AiChat;

public sealed record FinancialGoalDraftResponse(
    string Description,
    decimal TargetAmount,
    string TargetDate,
    int Priority);
