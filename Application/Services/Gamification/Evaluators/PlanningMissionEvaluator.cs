using Application.Planning;
using Application.Services.Gamification;
using Domain.Enums.Gamification;
using Domain.Interfaces.Gamification;
using Domain.Services.Gamification;

namespace Application.Services.Gamification.Evaluators;

public sealed class PlanningMissionEvaluator : IMissionEvaluator
{
    private const string HorizonClearStreakKey = "PlanningHorizonClearStreak";

    private readonly IProjectionRunner _projectionRunner;
    private readonly IConsistencyCounterRepository _counters;

    public PlanningMissionEvaluator(IProjectionRunner projectionRunner, IConsistencyCounterRepository counters)
    {
        _projectionRunner = projectionRunner;
        _counters = counters;
    }

    public async Task<IReadOnlyList<MissionEvaluationResult>> EvaluateAsync(int userId, DateTime asOf)
    {
        var context = await _projectionRunner.RunAsync(userId, asOf, PlanningDefaults.DefaultHorizonMonths, null);
        var isClear = context.Projection.FirstNegativeMonth is null;

        await ConsistencyCounterUpdater.UpdateStreakAsync(_counters, userId, HorizonClearStreakKey, isClear);

        return new List<MissionEvaluationResult>
        {
            new(MissionEventType.PlanningHorizonClear, null, isClear),
            new(MissionEventType.PlanningHorizonAlert, null, !isClear)
        };
    }
}
