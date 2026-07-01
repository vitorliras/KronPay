namespace Application.DTOs.Planning;

public sealed record CreatePlannedCommitmentRequest(
    string Description,
    decimal Amount,
    string Direction,
    string Periodicity,
    DateTime StartDate,
    DateTime? EndDate,
    int? CategoryId);
