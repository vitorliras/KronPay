namespace Domain.Enums.Gamification;

public enum MissionEventType
{
    BudgetMonthClosed,
    BudgetQuarterFlawless,
    BudgetLimitExceeded,
    BudgetRelapse,
    BudgetLimitRealisticAdjustment,
    BudgetLimitInflated,
    BudgetAllCategoriesOnTrack,
    BudgetNewCategoryControlled,
    BudgetWideCoverage,

    CardInvoiceOnTime,
    CardNoLateInvoiceSemester,
    CardInvoiceLate,
    CardConsciousLimitUsage,
    CardAtLimit,
    CardInvoiceFullyPaid,

    GoalContributionOnSchedule,
    GoalAchieved,
    GoalContributionForgotten,
    GoalLost,
    GoalAchievedEarly,
    GoalRetriedAndAchieved,
    GoalAmbitiousAchieved,
    GoalAbandoned,

    PlanningHorizonClear,
    PlanningHorizonAlert,
    PlanningForecastMet,
    PlanningDivergedFromForecast,

    SpendingCategoryUnderControl,
    SpendingCategoryRising,
    SpendingTrendReversed,

    NotificationResolvedQuickly,
    NotificationCriticalIgnored,

    TransactionMonthSurplus,
    TransactionMonthDeficit,
    TransactionFullyCategorized,
    TransactionUncategorizedPileup,
    InvestmentTwentyPercentPlus,
    InvestmentTenToTwentyPercent,
    InvestmentBelowTenPercent,
    InvestmentWithdrawal,
    TransactionPositiveStreak,
    TotalSpendingDecline,
    TotalSpendingSpike,
    SpendingConcentration,
    FiscalYearPositive,
    PersonalBestMonth,
    IncomeDiversified,
    ReserveMaintained
}
