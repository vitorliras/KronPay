using Domain.Entities.Planning;

namespace Domain.Interfaces.Planning;

public interface IPlannedCommitmentRepository
{
    Task<bool> AddAsync(PlannedCommitment commitment);
    bool Update(PlannedCommitment commitment);
    Task<PlannedCommitment?> GetByIdAsync(int id, int userId);
    Task<IEnumerable<PlannedCommitment>> GetByUserAsync(int userId);
    Task<IReadOnlyList<PlannedCommitment>> GetDeactivatedOlderThanAsync(DateTime cutoff);
    Task DeleteRangeAsync(IEnumerable<PlannedCommitment> commitments);
}
