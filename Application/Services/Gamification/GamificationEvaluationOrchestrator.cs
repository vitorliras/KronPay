using Application.Services.Gamification.Evaluators;
using Domain.interfaces;
using Domain.Interfaces;
using Domain.Interfaces.Gamification;
using Domain.Services.Gamification;
using Microsoft.Extensions.Logging;

namespace Application.Services.Gamification;

public sealed class GamificationEvaluationOrchestrator : IGamificationEvaluationOrchestrator
{
    private const string AnyMissionTriggeredKey = "AnyMissionTriggered";

    private readonly IUserRepository _users;
    private readonly IConsistencyCounterRepository _counters;
    private readonly GamificationMissionApplier _applier;
    private readonly IReadOnlyList<IMissionEvaluator> _evaluators;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<GamificationEvaluationOrchestrator> _logger;

    public GamificationEvaluationOrchestrator(
        IUserRepository users,
        IConsistencyCounterRepository counters,
        GamificationMissionApplier applier,
        BudgetMissionEvaluator budgetEvaluator,
        CardMissionEvaluator cardEvaluator,
        GoalMissionEvaluator goalEvaluator,
        PlanningMissionEvaluator planningEvaluator,
        SpendingTrendMissionEvaluator spendingTrendEvaluator,
        NotificationMissionEvaluator notificationEvaluator,
        TransactionMissionEvaluator transactionEvaluator,
        IUnitOfWork uow,
        ILogger<GamificationEvaluationOrchestrator> logger)
    {
        _users = users;
        _counters = counters;
        _applier = applier;
        _evaluators = new IMissionEvaluator[]
        {
            budgetEvaluator,
            cardEvaluator,
            goalEvaluator,
            planningEvaluator,
            spendingTrendEvaluator,
            notificationEvaluator,
            transactionEvaluator
        };
        _uow = uow;
        _logger = logger;
    }

    public async Task<GamificationEvaluationSummary> RunAsync()
    {
        var userIds = await _users.GetAllUserIdsAsync();
        var eventsTriggered = 0;
        var badgesUnlocked = 0;
        var asOf = DateTime.UtcNow;

        foreach (var userId in userIds)
        {
            var anyTransition = false;

            foreach (var evaluator in _evaluators)
            {
                var results = await evaluator.EvaluateAsync(userId, asOf);

                foreach (var result in results)
                {
                    var wasTriggered = await _applier.ApplyResultAsync(userId, result);
                    if (wasTriggered)
                    {
                        eventsTriggered++;
                        anyTransition = true;
                    }
                }
            }

            if (anyTransition)
                await ConsistencyCounterUpdater.MarkOnceAsync(_counters, userId, AnyMissionTriggeredKey);

            var newBadges = await _applier.UnlockEligibleBadgesAsync(userId);
            badgesUnlocked += newBadges.Count;
        }

        if (!await _uow.CommitAsync())
            _logger.LogError("Falha ao persistir a avaliação de gamificação ({AsOf}).", asOf);

        return new GamificationEvaluationSummary(userIds.Count, eventsTriggered, badgesUnlocked);
    }
}
