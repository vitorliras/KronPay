using Domain.Enums.Gamification;
using Shared.Localization;

namespace Application.Gamification;

public sealed record MissionCatalogEntry(
    string Area,
    MissionEventSignificance Significance,
    bool IsGain,
    string MessageKey);

public static class MissionCatalog
{
    private const string Budget = "Orçamento";
    private const string Card = "Cartão de Crédito";
    private const string Goals = "Metas";
    private const string Planning = "Planejamento";
    private const string SpendingTrend = "Tendência de Gastos";
    private const string Notifications = "Notificações";
    private const string Transactions = "Transações";

    private static readonly IReadOnlyDictionary<MissionEventType, MissionCatalogEntry> Entries =
        new Dictionary<MissionEventType, MissionCatalogEntry>
        {
            [MissionEventType.BudgetMonthClosed] = new(Budget, MissionEventSignificance.Medio, true, MessageKeys.MissionBudgetMonthClosed),
            [MissionEventType.BudgetQuarterFlawless] = new(Budget, MissionEventSignificance.Grande, true, MessageKeys.MissionBudgetQuarterFlawless),
            [MissionEventType.BudgetLimitExceeded] = new(Budget, MissionEventSignificance.Medio, false, MessageKeys.MissionBudgetLimitExceeded),
            [MissionEventType.BudgetRelapse] = new(Budget, MissionEventSignificance.Grande, false, MessageKeys.MissionBudgetRelapse),
            [MissionEventType.BudgetLimitRealisticAdjustment] = new(Budget, MissionEventSignificance.Pequeno, true, MessageKeys.MissionBudgetLimitRealisticAdjustment),
            [MissionEventType.BudgetLimitInflated] = new(Budget, MissionEventSignificance.Medio, false, MessageKeys.MissionBudgetLimitInflated),
            [MissionEventType.BudgetAllCategoriesOnTrack] = new(Budget, MissionEventSignificance.Grande, true, MessageKeys.MissionBudgetAllCategoriesOnTrack),
            [MissionEventType.BudgetNewCategoryControlled] = new(Budget, MissionEventSignificance.Pequeno, true, MessageKeys.MissionBudgetNewCategoryControlled),
            [MissionEventType.BudgetWideCoverage] = new(Budget, MissionEventSignificance.Medio, true, MessageKeys.MissionBudgetWideCoverage),

            [MissionEventType.CardInvoiceOnTime] = new(Card, MissionEventSignificance.Pequeno, true, MessageKeys.MissionCardInvoiceOnTime),
            [MissionEventType.CardNoLateInvoiceSemester] = new(Card, MissionEventSignificance.Grande, true, MessageKeys.MissionCardNoLateInvoiceSemester),
            [MissionEventType.CardInvoiceLate] = new(Card, MissionEventSignificance.Medio, false, MessageKeys.MissionCardInvoiceLate),
            [MissionEventType.CardConsciousLimitUsage] = new(Card, MissionEventSignificance.Pequeno, true, MessageKeys.MissionCardConsciousLimitUsage),
            [MissionEventType.CardAtLimit] = new(Card, MissionEventSignificance.Pequeno, false, MessageKeys.MissionCardAtLimit),
            [MissionEventType.CardInvoiceFullyPaid] = new(Card, MissionEventSignificance.Medio, true, MessageKeys.MissionCardInvoiceFullyPaid),

            [MissionEventType.GoalContributionOnSchedule] = new(Goals, MissionEventSignificance.Pequeno, true, MessageKeys.MissionGoalContributionOnSchedule),
            [MissionEventType.GoalAchieved] = new(Goals, MissionEventSignificance.Grande, true, MessageKeys.MissionGoalAchieved),
            [MissionEventType.GoalContributionForgotten] = new(Goals, MissionEventSignificance.Medio, false, MessageKeys.MissionGoalContributionForgotten),
            [MissionEventType.GoalLost] = new(Goals, MissionEventSignificance.Grande, false, MessageKeys.MissionGoalLost),
            [MissionEventType.GoalAchievedEarly] = new(Goals, MissionEventSignificance.Grande, true, MessageKeys.MissionGoalAchievedEarly),
            [MissionEventType.GoalRetriedAndAchieved] = new(Goals, MissionEventSignificance.Medio, true, MessageKeys.MissionGoalRetriedAndAchieved),
            [MissionEventType.GoalAmbitiousAchieved] = new(Goals, MissionEventSignificance.Grande, true, MessageKeys.MissionGoalAmbitiousAchieved),
            [MissionEventType.GoalAbandoned] = new(Goals, MissionEventSignificance.Medio, false, MessageKeys.MissionGoalAbandoned),

            [MissionEventType.PlanningHorizonClear] = new(Planning, MissionEventSignificance.Medio, true, MessageKeys.MissionPlanningHorizonClear),
            [MissionEventType.PlanningHorizonAlert] = new(Planning, MissionEventSignificance.Medio, false, MessageKeys.MissionPlanningHorizonAlert),
            [MissionEventType.PlanningForecastMet] = new(Planning, MissionEventSignificance.Medio, true, MessageKeys.MissionPlanningForecastMet),
            [MissionEventType.PlanningDivergedFromForecast] = new(Planning, MissionEventSignificance.Medio, false, MessageKeys.MissionPlanningDivergedFromForecast),

            [MissionEventType.SpendingCategoryUnderControl] = new(SpendingTrend, MissionEventSignificance.Pequeno, true, MessageKeys.MissionSpendingCategoryUnderControl),
            [MissionEventType.SpendingCategoryRising] = new(SpendingTrend, MissionEventSignificance.Pequeno, false, MessageKeys.MissionSpendingCategoryRising),
            [MissionEventType.SpendingTrendReversed] = new(SpendingTrend, MissionEventSignificance.Medio, true, MessageKeys.MissionSpendingTrendReversed),

            [MissionEventType.NotificationResolvedQuickly] = new(Notifications, MissionEventSignificance.Pequeno, true, MessageKeys.MissionNotificationResolvedQuickly),
            [MissionEventType.NotificationCriticalIgnored] = new(Notifications, MissionEventSignificance.Pequeno, false, MessageKeys.MissionNotificationCriticalIgnored),

            [MissionEventType.TransactionMonthSurplus] = new(Transactions, MissionEventSignificance.Grande, true, MessageKeys.MissionTransactionMonthSurplus),
            [MissionEventType.TransactionMonthDeficit] = new(Transactions, MissionEventSignificance.Grande, false, MessageKeys.MissionTransactionMonthDeficit),
            [MissionEventType.TransactionFullyCategorized] = new(Transactions, MissionEventSignificance.Pequeno, true, MessageKeys.MissionTransactionFullyCategorized),
            [MissionEventType.TransactionUncategorizedPileup] = new(Transactions, MissionEventSignificance.Pequeno, false, MessageKeys.MissionTransactionUncategorizedPileup),
            [MissionEventType.InvestmentTwentyPercentPlus] = new(Transactions, MissionEventSignificance.Grande, true, MessageKeys.MissionInvestmentTwentyPercentPlus),
            [MissionEventType.InvestmentTenToTwentyPercent] = new(Transactions, MissionEventSignificance.Medio, true, MessageKeys.MissionInvestmentTenToTwentyPercent),
            [MissionEventType.InvestmentBelowTenPercent] = new(Transactions, MissionEventSignificance.Pequeno, false, MessageKeys.MissionInvestmentBelowTenPercent),
            [MissionEventType.InvestmentWithdrawal] = new(Transactions, MissionEventSignificance.Medio, false, MessageKeys.MissionInvestmentWithdrawal),
            [MissionEventType.TransactionPositiveStreak] = new(Transactions, MissionEventSignificance.Grande, true, MessageKeys.MissionTransactionPositiveStreak),
            [MissionEventType.TotalSpendingDecline] = new(Transactions, MissionEventSignificance.Medio, true, MessageKeys.MissionTotalSpendingDecline),
            [MissionEventType.TotalSpendingSpike] = new(Transactions, MissionEventSignificance.Medio, false, MessageKeys.MissionTotalSpendingSpike),
            [MissionEventType.SpendingConcentration] = new(Transactions, MissionEventSignificance.Pequeno, false, MessageKeys.MissionSpendingConcentration),
            [MissionEventType.FiscalYearPositive] = new(Transactions, MissionEventSignificance.Grande, true, MessageKeys.MissionFiscalYearPositive),
            [MissionEventType.PersonalBestMonth] = new(Transactions, MissionEventSignificance.Medio, true, MessageKeys.MissionPersonalBestMonth),
            [MissionEventType.IncomeDiversified] = new(Transactions, MissionEventSignificance.Pequeno, true, MessageKeys.MissionIncomeDiversified),
            [MissionEventType.ReserveMaintained] = new(Transactions, MissionEventSignificance.Medio, true, MessageKeys.MissionReserveMaintained)
        };

    public static MissionCatalogEntry Get(MissionEventType type) => Entries[type];

    public static IReadOnlyDictionary<MissionEventType, MissionCatalogEntry> All => Entries;
}
