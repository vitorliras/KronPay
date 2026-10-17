namespace Application.DTOs.AiChat;

public sealed record PlannedCommitmentDraftResponse(
    string Description,
    decimal Amount,
    string Direction,
    string Periodicity,
    string StartDate,
    string? EndDate,
    int? CategoryId);
