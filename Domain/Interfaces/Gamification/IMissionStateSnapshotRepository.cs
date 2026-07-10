using Domain.Entities.Gamification;
using Domain.Enums.Gamification;

namespace Domain.Interfaces.Gamification;

public interface IMissionStateSnapshotRepository
{
    Task<MissionStateSnapshot?> GetAsync(int userId, MissionEventType type, int? relatedEntityId);
    Task<IEnumerable<MissionStateSnapshot>> GetAllByUserAsync(int userId);
    Task<bool> AddAsync(MissionStateSnapshot snapshot);
    bool Update(MissionStateSnapshot snapshot);
}
