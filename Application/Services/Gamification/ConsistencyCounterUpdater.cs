using Domain.Entities.Gamification;
using Domain.Interfaces.Gamification;

namespace Application.Services.Gamification;

public static class ConsistencyCounterUpdater
{
    public static async Task<int> UpdateStreakAsync(
        IConsistencyCounterRepository counters, int userId, string key, bool condition)
    {
        var counter = await counters.GetAsync(userId, key);

        if (counter is null)
        {
            counter = new ConsistencyCounter(userId, key);
            if (condition)
                counter.Increment();

            await counters.AddAsync(counter);
            return counter.CurrentStreak;
        }

        if (condition)
            counter.Increment();
        else
            counter.Reset();

        counters.Update(counter);
        return counter.CurrentStreak;
    }

    public static async Task IncrementOnlyAsync(IConsistencyCounterRepository counters, int userId, string key)
    {
        var counter = await counters.GetAsync(userId, key);

        if (counter is null)
        {
            counter = new ConsistencyCounter(userId, key);
            counter.Increment();
            await counters.AddAsync(counter);
            return;
        }

        counter.Increment();
        counters.Update(counter);
    }

    public static async Task RecordValueAsync(IConsistencyCounterRepository counters, int userId, string key, int value)
    {
        var counter = await counters.GetAsync(userId, key);

        if (counter is null)
        {
            counter = new ConsistencyCounter(userId, key);
            counter.SetValue(value);
            await counters.AddAsync(counter);
            return;
        }

        counter.SetValue(value);
        counters.Update(counter);
    }

    public static async Task<bool> MarkFirstOccurrenceAsync(IConsistencyCounterRepository counters, int userId, string key)
    {
        var counter = await counters.GetAsync(userId, key);

        if (counter is null)
        {
            counter = new ConsistencyCounter(userId, key);
            counter.SetValue(1);
            await counters.AddAsync(counter);
            return true;
        }

        if (counter.CurrentStreak >= 1)
            return false;

        counter.SetValue(1);
        counters.Update(counter);
        return true;
    }

    public static async Task MarkOnceAsync(IConsistencyCounterRepository counters, int userId, string key)
    {
        var counter = await counters.GetAsync(userId, key);

        if (counter is null)
        {
            counter = new ConsistencyCounter(userId, key);
            counter.SetValue(1);
            await counters.AddAsync(counter);
            return;
        }

        if (counter.CurrentStreak < 1)
        {
            counter.SetValue(1);
            counters.Update(counter);
        }
    }
}
