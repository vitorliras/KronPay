namespace Domain.Entities.Gamification;

public sealed class GamificationEvaluationRun
{
    public int Id { get; private set; }
    public DateTime RanAt { get; private set; }
    public int UsersProcessed { get; private set; }
    public int EventsTriggered { get; private set; }
    public int BadgesUnlocked { get; private set; }

    public GamificationEvaluationRun()
    {
        RanAt = DateTime.MinValue;
    }

    public void MarkRun(DateTime when, int usersProcessed, int eventsTriggered, int badgesUnlocked)
    {
        RanAt = when;
        UsersProcessed = usersProcessed;
        EventsTriggered = eventsTriggered;
        BadgesUnlocked = badgesUnlocked;
    }
}
