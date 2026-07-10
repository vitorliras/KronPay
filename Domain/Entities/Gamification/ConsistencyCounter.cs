using Domain.Exceptions;
using Shared.Localization;

namespace Domain.Entities.Gamification;

public sealed class ConsistencyCounter
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public string CounterKey { get; private set; } = default!;
    public int CurrentStreak { get; private set; }
    public int BestStreak { get; private set; }
    public DateTime LastUpdatedAt { get; private set; }

    protected ConsistencyCounter() { }

    public ConsistencyCounter(int userId, string counterKey)
    {
        if (userId <= 0)
            throw new DomainException(MessageKeys.InvaldUser);

        UserId = userId;
        CounterKey = counterKey;
        CurrentStreak = 0;
        BestStreak = 0;
        LastUpdatedAt = DateTime.UtcNow;
    }

    public void Increment()
    {
        CurrentStreak++;
        if (CurrentStreak > BestStreak)
            BestStreak = CurrentStreak;

        LastUpdatedAt = DateTime.UtcNow;
    }

    public void SetValue(int value)
    {
        CurrentStreak = value;
        if (CurrentStreak > BestStreak)
            BestStreak = CurrentStreak;

        LastUpdatedAt = DateTime.UtcNow;
    }

    public void Reset()
    {
        CurrentStreak = 0;
        LastUpdatedAt = DateTime.UtcNow;
    }
}
