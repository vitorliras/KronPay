using Application.Services.Gamification;
using Domain.Entities.Goals;
using Domain.Enums.Gamification;
using Domain.Enums.Goals;
using Domain.Interfaces.Gamification;
using Domain.Interfaces.Goals;
using Domain.Services.Gamification;

namespace Application.Services.Gamification.Evaluators;

public sealed class GoalMissionEvaluator : IMissionEvaluator
{
    private const int ContributionStaleDays = 45;
    private const decimal AmbitiousMultiplier = 1.5m;
    private const string ActiveGoalStreakKey = "HasActiveGoalStreak";
    private const string GoalAchievedCountKey = "GoalAchievedCount";
    private const string HasGoalKey = "HasGoal";
    private const string HasGoalAchievedKey = "HasGoalAchieved";
    private const string HasAmbitiousGoalAchievedKey = "HasAmbitiousGoalAchieved";
    private const string HasRetriedGoalAchievedKey = "HasRetriedGoalAchieved";

    private readonly IFinancialGoalRepository _goals;
    private readonly IConsistencyCounterRepository _counters;

    public GoalMissionEvaluator(IFinancialGoalRepository goals, IConsistencyCounterRepository counters)
    {
        _goals = goals;
        _counters = counters;
    }

    public async Task<IReadOnlyList<MissionEvaluationResult>> EvaluateAsync(int userId, DateTime asOf)
    {
        var results = new List<MissionEvaluationResult>();

        var active = (await _goals.GetActiveAsync(userId)).ToList();
        var history = (await _goals.GetHistoryAsync(userId, null)).ToList();
        var allGoals = active.Concat(history).GroupBy(g => g.Id).Select(g => g.First()).ToList();

        if (allGoals.Count > 0)
            await ConsistencyCounterUpdater.MarkOnceAsync(_counters, userId, HasGoalKey);

        foreach (var goal in active)
        {
            var referenceDate = goal.LastContributionAt ?? goal.CreatedAt;
            var onSchedule = (asOf - referenceDate).TotalDays <= ContributionStaleDays;

            results.Add(new MissionEvaluationResult(MissionEventType.GoalContributionOnSchedule, goal.Id, onSchedule));
            results.Add(new MissionEvaluationResult(MissionEventType.GoalContributionForgotten, goal.Id, !onSchedule));
        }

        await ConsistencyCounterUpdater.UpdateStreakAsync(_counters, userId, ActiveGoalStreakKey, active.Count > 0);

        foreach (var goal in history.Where(g => g.Status != FinancialGoalStatus.Active))
        {
            if (goal.Status == FinancialGoalStatus.Completed)
            {
                var isEarly = goal.CompletedAt.HasValue && goal.CompletedAt.Value.Date < goal.TargetDate.Date;
                var isRetried = goal.PreviousAttemptGoalId.HasValue;
                var isAmbitious = IsAmbitious(goal, allGoals);

                results.Add(new MissionEvaluationResult(MissionEventType.GoalAchieved, goal.Id, true));
                results.Add(new MissionEvaluationResult(MissionEventType.GoalAchievedEarly, goal.Id, isEarly));
                results.Add(new MissionEvaluationResult(MissionEventType.GoalRetriedAndAchieved, goal.Id, isRetried));
                results.Add(new MissionEvaluationResult(MissionEventType.GoalAmbitiousAchieved, goal.Id, isAmbitious));

                await ConsistencyCounterUpdater.MarkOnceAsync(_counters, userId, HasGoalAchievedKey);

                var isFirstTimeCounted = await ConsistencyCounterUpdater.MarkFirstOccurrenceAsync(
                    _counters, userId, $"GoalAchievedCounted:GoalId={goal.Id}");
                if (isFirstTimeCounted)
                    await ConsistencyCounterUpdater.IncrementOnlyAsync(_counters, userId, GoalAchievedCountKey);

                if (isAmbitious)
                    await ConsistencyCounterUpdater.MarkOnceAsync(_counters, userId, HasAmbitiousGoalAchievedKey);

                if (isRetried)
                    await ConsistencyCounterUpdater.MarkOnceAsync(_counters, userId, HasRetriedGoalAchievedKey);
            }
            else if (goal.Status == FinancialGoalStatus.Expired)
            {
                results.Add(new MissionEvaluationResult(MissionEventType.GoalLost, goal.Id, true));
            }
        }

        return results;
    }

    private static bool IsAmbitious(FinancialGoal goal, IReadOnlyList<FinancialGoal> allGoals)
    {
        var others = allGoals.Where(g => g.Id != goal.Id).Select(g => g.TargetAmount).ToList();
        if (others.Count == 0)
            return false;

        var average = others.Average();
        return average > 0 && goal.TargetAmount > average * AmbitiousMultiplier;
    }
}
