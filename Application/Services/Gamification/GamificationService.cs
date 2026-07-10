using Application.Services.Gamification.Evaluators;
using Domain.Enums.Gamification;
using Domain.Interfaces.Gamification;
using Domain.Services.Gamification;

namespace Application.Services.Gamification;

public sealed class GamificationService : IGamificationService
{
    private const string HasChattedWithAssistantKey = "HasChattedWithAssistant";

    private readonly IReadOnlyDictionary<MissionEventType, IMissionEvaluator> _evaluatorsByType;
    private readonly GamificationMissionApplier _applier;
    private readonly IUserBadgeRepository _userBadges;
    private readonly IConsistencyCounterRepository _counters;

    public GamificationService(
        BudgetMissionEvaluator budgetEvaluator,
        CardMissionEvaluator cardEvaluator,
        GoalMissionEvaluator goalEvaluator,
        PlanningMissionEvaluator planningEvaluator,
        SpendingTrendMissionEvaluator spendingTrendEvaluator,
        NotificationMissionEvaluator notificationEvaluator,
        TransactionMissionEvaluator transactionEvaluator,
        GamificationMissionApplier applier,
        IUserBadgeRepository userBadges,
        IConsistencyCounterRepository counters)
    {
        _evaluatorsByType = BuildEvaluatorMap(
            budgetEvaluator, cardEvaluator, goalEvaluator, planningEvaluator,
            spendingTrendEvaluator, notificationEvaluator, transactionEvaluator);
        _applier = applier;
        _userBadges = userBadges;
        _counters = counters;
    }

    public async Task TriggerInstantEvaluationAsync(int userId, MissionEventType type, int? relatedEntityId)
    {
        if (!_evaluatorsByType.TryGetValue(type, out var evaluator))
            return;

        var results = await evaluator.EvaluateAsync(userId, DateTime.UtcNow);

        foreach (var result in results)
            await _applier.ApplyResultAsync(userId, result);

        await _applier.UnlockEligibleBadgesAsync(userId);
    }

    public async Task<bool> HasUnlockedBadgeAsync(int userId, BadgeCode code)
        => await _userBadges.ExistsAsync(userId, code);

    public async Task NotifyAssistantConversationAsync(int userId)
    {
        await ConsistencyCounterUpdater.MarkOnceAsync(_counters, userId, HasChattedWithAssistantKey);
        await _applier.UnlockEligibleBadgesAsync(userId);
    }

    private static IReadOnlyDictionary<MissionEventType, IMissionEvaluator> BuildEvaluatorMap(
        BudgetMissionEvaluator budgetEvaluator,
        CardMissionEvaluator cardEvaluator,
        GoalMissionEvaluator goalEvaluator,
        PlanningMissionEvaluator planningEvaluator,
        SpendingTrendMissionEvaluator spendingTrendEvaluator,
        NotificationMissionEvaluator notificationEvaluator,
        TransactionMissionEvaluator transactionEvaluator)
    {
        var map = new Dictionary<MissionEventType, IMissionEvaluator>();

        void AddRange(IMissionEvaluator evaluator, params MissionEventType[] types)
        {
            foreach (var type in types)
                map[type] = evaluator;
        }

        AddRange(budgetEvaluator,
            MissionEventType.BudgetMonthClosed, MissionEventType.BudgetQuarterFlawless,
            MissionEventType.BudgetLimitExceeded, MissionEventType.BudgetRelapse,
            MissionEventType.BudgetLimitRealisticAdjustment, MissionEventType.BudgetLimitInflated,
            MissionEventType.BudgetAllCategoriesOnTrack, MissionEventType.BudgetNewCategoryControlled,
            MissionEventType.BudgetWideCoverage);

        AddRange(cardEvaluator,
            MissionEventType.CardInvoiceOnTime, MissionEventType.CardNoLateInvoiceSemester,
            MissionEventType.CardInvoiceLate, MissionEventType.CardConsciousLimitUsage,
            MissionEventType.CardAtLimit, MissionEventType.CardInvoiceFullyPaid);

        AddRange(goalEvaluator,
            MissionEventType.GoalContributionOnSchedule, MissionEventType.GoalAchieved,
            MissionEventType.GoalContributionForgotten, MissionEventType.GoalLost,
            MissionEventType.GoalAchievedEarly, MissionEventType.GoalRetriedAndAchieved,
            MissionEventType.GoalAmbitiousAchieved, MissionEventType.GoalAbandoned);

        AddRange(planningEvaluator,
            MissionEventType.PlanningHorizonClear, MissionEventType.PlanningHorizonAlert,
            MissionEventType.PlanningForecastMet, MissionEventType.PlanningDivergedFromForecast);

        AddRange(spendingTrendEvaluator,
            MissionEventType.SpendingCategoryUnderControl, MissionEventType.SpendingCategoryRising,
            MissionEventType.SpendingTrendReversed);

        AddRange(notificationEvaluator,
            MissionEventType.NotificationResolvedQuickly, MissionEventType.NotificationCriticalIgnored);

        AddRange(transactionEvaluator,
            MissionEventType.TransactionMonthSurplus, MissionEventType.TransactionMonthDeficit,
            MissionEventType.TransactionFullyCategorized, MissionEventType.TransactionUncategorizedPileup,
            MissionEventType.InvestmentTwentyPercentPlus, MissionEventType.InvestmentTenToTwentyPercent,
            MissionEventType.InvestmentBelowTenPercent, MissionEventType.InvestmentWithdrawal,
            MissionEventType.TransactionPositiveStreak, MissionEventType.TotalSpendingDecline,
            MissionEventType.TotalSpendingSpike, MissionEventType.SpendingConcentration,
            MissionEventType.FiscalYearPositive, MissionEventType.PersonalBestMonth,
            MissionEventType.IncomeDiversified, MissionEventType.ReserveMaintained);

        return map;
    }
}
