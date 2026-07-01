namespace Application.DTOs.Planning;

public sealed record PlannedCommitmentResponse(
    int Id,
    string Description,
    decimal Amount,
    string Direction,
    string Periodicity,
    DateTime StartDate,
    DateTime? EndDate,
    int? CategoryId,
    bool Active);
