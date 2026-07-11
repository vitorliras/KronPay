using Domain.Entities.DataRetention;

namespace Domain.Interfaces.DataRetention;

public interface IDataRetentionPurgeRunRepository
{
    Task<DataRetentionPurgeRun?> GetAsync();
    Task<bool> AddAsync(DataRetentionPurgeRun run);
    bool Update(DataRetentionPurgeRun run);
}
