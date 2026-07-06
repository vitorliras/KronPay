namespace Domain.Entities.Notifications;

public sealed class NotificationEvaluationRun
{
    public int Id { get; private set; }
    public DateTime LastRunAt { get; private set; }

    public NotificationEvaluationRun()
    {
        LastRunAt = DateTime.MinValue;
    }

    public void MarkRun(DateTime when)
    {
        LastRunAt = when;
    }
}
