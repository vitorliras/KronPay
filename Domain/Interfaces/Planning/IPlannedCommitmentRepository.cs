using Domain.Entities.Planning;

namespace Domain.Interfaces.Planning;

public interface IPlannedCommitmentRepository
{
    Task<bool> AddAsync(PlannedCommitment commitment);
    bool Update(PlannedCommitment commitment);
    Task<PlannedCommitment?> GetByIdAsync(int id, int userId);
    Task<IEnumerable<PlannedCommitment>> GetByUserAsync(int userId);
}
