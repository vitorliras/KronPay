using Application.DTOs.Planning;
using Domain.Entities.Planning;

namespace Application.Planning;

public static class PlannedCommitmentMapper
{
    public static PlannedCommitmentResponse ToResponse(PlannedCommitment commitment)
        => new(
            commitment.Id,
            commitment.Description,
            commitment.Amount,
            commitment.Direction,
            commitment.Periodicity,
            commitment.StartDate,
            commitment.EndDate,
            commitment.CategoryId,
            commitment.Active);
}
