namespace Domain.Entities.DataRetention;

public sealed class DataRetentionPurgeRun
{
    public int Id { get; private set; }
    public DateTime LastRunAt { get; private set; }

    public DataRetentionPurgeRun()
    {
        LastRunAt = DateTime.MinValue;
    }

    public void MarkRun(DateTime when)
    {
        LastRunAt = when;
    }
}
