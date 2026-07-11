namespace Application.DataRetention;

public interface IRetentionPurgeTarget
{
    Task<int> PurgeAsync(DateTime cutoff);
}
