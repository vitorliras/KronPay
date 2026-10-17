using Domain.Entities.AI;

namespace Domain.Interfaces.AI;

public interface IAiUsageLogRepository
{
    Task<bool> AddAsync(AiUsageLog log);
    Task<IReadOnlyList<AiUsageLog>> GetByPeriodAsync(int userId, DateTime fromInclusive, DateTime toExclusive);
    Task<int> CountByFeatureSinceAsync(int userId, string feature, DateTime sinceInclusive);
}
