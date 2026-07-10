using Domain.Entities.Gamification;

namespace Domain.Interfaces.Gamification;

public interface IConsistencyCounterRepository
{
    Task<ConsistencyCounter?> GetAsync(int userId, string counterKey);
    Task<IEnumerable<ConsistencyCounter>> GetAllByUserAsync(int userId);
    Task<bool> AddAsync(ConsistencyCounter counter);
    bool Update(ConsistencyCounter counter);
}
