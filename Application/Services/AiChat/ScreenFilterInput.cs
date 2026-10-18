namespace Application.Services.AiChat;

public sealed record ScreenFilterInput(
    string? Tab = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    int? Month = null,
    int? Year = null,
    string? Type = null,
    int? CategoryId = null,
    string? Search = null,
    int? HorizonMonths = null,
    int? CardId = null,
    int? GoalId = null);
