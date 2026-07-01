namespace Application.DTOs.Planning;

public sealed record UpdatePlannedCommitmentRequest(
    int Id,
    string Description,
    decimal Amount,
    string Direction,
    string Periodicity,
    DateTime StartDate,
    DateTime? EndDate,
    int? CategoryId);
