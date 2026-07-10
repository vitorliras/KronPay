using Application.Gamification;
using Domain.Enums.Gamification;
using Domain.Interfaces.Gamification;

namespace Application.Services.Gamification;

public sealed class BadgeUnlockEvaluator
{
    private sealed record CounterRule(BadgeCode Code, string Key, int Threshold, bool IsPrefix);

    private static readonly IReadOnlyList<CounterRule> Rules = new List<CounterRule>
    {
        new(BadgeCode.FirstSteps, "HasTransaction", 1, false),
        new(BadgeCode.BreakingTheIce, "HasChattedWithAssistant", 1, false),
        new(BadgeCode.FirstGoal, "HasGoal", 1, false),
        new(BadgeCode.AllConnected, "HasCreditCard", 1, false),
        new(BadgeCode.FirstMission, "AnyMissionTriggered", 1, false),
        new(BadgeCode.MultipleCards, "CreditCardCount", 2, false),
        new(BadgeCode.FirstPositiveMonth, "HasPositiveMonth", 1, false),
        new(BadgeCode.FirstInvoiceOnTime, "CardOnTime:CreditCardId=", 1, true),
        new(BadgeCode.OrganizedMonth, "HasFullyCategorizedMonth", 1, false),
        new(BadgeCode.FirstGoalAchieved, "HasGoalAchieved", 1, false),
        new(BadgeCode.FirstInvestment, "HasInvestment", 1, false),
        new(BadgeCode.NoPendingAlerts, "HasZeroCriticalAlerts", 1, false),
        new(BadgeCode.FirstBudgetMet, "HasBudgetMet", 1, false),

        new(BadgeCode.IronDiscipline, "BudgetAdherence:CategoryId=", 6, true),
        new(BadgeCode.FlawlessQuarter, "BudgetAdherence:CategoryId=", 3, true),
        new(BadgeCode.ConsistentInvestor, "InvestmentTwentyPlusStreak", 3, false),
        new(BadgeCode.NoLatePayments, "CardOnTime:CreditCardId=", 6, true),
        new(BadgeCode.QuarterInTheBlack, "HasMonthSurplus", 1, false),
        new(BadgeCode.DoubleGoalAchiever, "GoalAchievedCount", 2, false),
        new(BadgeCode.NoRelapses, "ZeroRelapseMonths", 6, false),
        new(BadgeCode.QuickResponder, "NotificationQuickResolveCount", 5, false),
        new(BadgeCode.TrendReverser, "TrendReversedCount", 2, false),
        new(BadgeCode.WideBudgetCoverage, "HasWideBudgetCoverage", 1, false),
        new(BadgeCode.RecoveredGoal, "HasRetriedGoalAchieved", 1, false),
        new(BadgeCode.SixMonthsFocused, "HasActiveGoalStreak", 6, false),

        new(BadgeCode.PerfectYear, "ZeroRelapseMonths", 12, false),
        new(BadgeCode.DedicatedInvestor, "InvestmentTwentyPlusStreak", 12, false),
        new(BadgeCode.PositiveFiscalYear, "HasFiscalYearPositive", 1, false),
        new(BadgeCode.IronDisciplineIII, "BudgetAdherence:CategoryId=", 24, true),
        new(BadgeCode.GoldenStreak, "PositiveMonthsStreak", 6, false),
        new(BadgeCode.AmbitiousGoalAchieved, "HasAmbitiousGoalAchieved", 1, false),
        new(BadgeCode.NoLatePaymentsYear, "CardOnTime:CreditCardId=", 12, true),
        new(BadgeCode.PlanningLegend, "PlanningHorizonClearStreak", 12, false)
    };

    private readonly IConsistencyCounterRepository _counters;
    private readonly IUserBadgeRepository _badges;

    public BadgeUnlockEvaluator(IConsistencyCounterRepository counters, IUserBadgeRepository badges)
    {
        _counters = counters;
        _badges = badges;
    }

    public async Task<IReadOnlyList<BadgeCode>> EvaluateAsync(int userId)
    {
        var unlockedCodes = (await _badges.GetByUserIdAsync(userId)).Select(b => b.Code).ToHashSet();
        var counters = (await _counters.GetAllByUserAsync(userId)).ToList();

        var newlyUnlocked = new List<BadgeCode>();

        foreach (var rule in Rules)
        {
            if (unlockedCodes.Contains(rule.Code))
                continue;

            var qualifies = rule.IsPrefix
                ? counters.Any(c => c.CounterKey.StartsWith(rule.Key) && c.BestStreak >= rule.Threshold)
                : (counters.FirstOrDefault(c => c.CounterKey == rule.Key)?.BestStreak ?? 0) >= rule.Threshold;

            if (qualifies)
                newlyUnlocked.Add(rule.Code);
        }

        if (!unlockedCodes.Contains(BadgeCode.CompleteCollection) && QualifiesForCompleteCollection(unlockedCodes, newlyUnlocked))
            newlyUnlocked.Add(BadgeCode.CompleteCollection);

        return newlyUnlocked;
    }

    private static bool QualifiesForCompleteCollection(HashSet<BadgeCode> unlockedCodes, IReadOnlyList<BadgeCode> newlyUnlocked)
    {
        var bronzeTotal = BadgeCatalog.All.Count(kv => kv.Value.Tier == BadgeTier.Bronze);
        var prataTotal = BadgeCatalog.All.Count(kv => kv.Value.Tier == BadgeTier.Prata);

        var allAccounted = unlockedCodes.Concat(newlyUnlocked).ToHashSet();
        var unlockedBronze = allAccounted.Count(c => BadgeCatalog.Get(c).Tier == BadgeTier.Bronze);
        var unlockedPrata = allAccounted.Count(c => BadgeCatalog.Get(c).Tier == BadgeTier.Prata);

        return unlockedBronze >= bronzeTotal && unlockedPrata >= prataTotal;
    }
}
