namespace Application.DataRetention;

public interface IDataRetentionPurgeOrchestrator
{
    Task<int> RunAsync();
}
