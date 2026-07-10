using Domain.Enums.Gamification;
using Shared.Localization;

namespace Application.Gamification;

public sealed record BadgeCatalogEntry(BadgeTier Tier, string MessageKey, string DescriptionMessageKey);

public static class BadgeCatalog
{
    private static readonly IReadOnlyDictionary<BadgeCode, BadgeCatalogEntry> Entries =
        new Dictionary<BadgeCode, BadgeCatalogEntry>
        {
            [BadgeCode.FirstSteps] = new(BadgeTier.Bronze, MessageKeys.BadgeFirstSteps, MessageKeys.BadgeFirstStepsDescription),
            [BadgeCode.BreakingTheIce] = new(BadgeTier.Bronze, MessageKeys.BadgeBreakingTheIce, MessageKeys.BadgeBreakingTheIceDescription),
            [BadgeCode.Explorer] = new(BadgeTier.Bronze, MessageKeys.BadgeExplorer, MessageKeys.BadgeExplorerDescription),
            [BadgeCode.FirstGoal] = new(BadgeTier.Bronze, MessageKeys.BadgeFirstGoal, MessageKeys.BadgeFirstGoalDescription),
            [BadgeCode.AllConnected] = new(BadgeTier.Bronze, MessageKeys.BadgeAllConnected, MessageKeys.BadgeAllConnectedDescription),
            [BadgeCode.FutureVision] = new(BadgeTier.Bronze, MessageKeys.BadgeFutureVision, MessageKeys.BadgeFutureVisionDescription),
            [BadgeCode.FirstCategory] = new(BadgeTier.Bronze, MessageKeys.BadgeFirstCategory, MessageKeys.BadgeFirstCategoryDescription),
            [BadgeCode.FirstMission] = new(BadgeTier.Bronze, MessageKeys.BadgeFirstMission, MessageKeys.BadgeFirstMissionDescription),
            [BadgeCode.MultipleCards] = new(BadgeTier.Bronze, MessageKeys.BadgeMultipleCards, MessageKeys.BadgeMultipleCardsDescription),
            [BadgeCode.FirstPositiveMonth] = new(BadgeTier.Bronze, MessageKeys.BadgeFirstPositiveMonth, MessageKeys.BadgeFirstPositiveMonthDescription),
            [BadgeCode.FirstInvoiceOnTime] = new(BadgeTier.Bronze, MessageKeys.BadgeFirstInvoiceOnTime, MessageKeys.BadgeFirstInvoiceOnTimeDescription),
            [BadgeCode.OrganizedMonth] = new(BadgeTier.Bronze, MessageKeys.BadgeOrganizedMonth, MessageKeys.BadgeOrganizedMonthDescription),
            [BadgeCode.FirstGoalAchieved] = new(BadgeTier.Bronze, MessageKeys.BadgeFirstGoalAchieved, MessageKeys.BadgeFirstGoalAchievedDescription),
            [BadgeCode.FirstInvestment] = new(BadgeTier.Bronze, MessageKeys.BadgeFirstInvestment, MessageKeys.BadgeFirstInvestmentDescription),
            [BadgeCode.NoPendingAlerts] = new(BadgeTier.Bronze, MessageKeys.BadgeNoPendingAlerts, MessageKeys.BadgeNoPendingAlertsDescription),
            [BadgeCode.FirstBudgetMet] = new(BadgeTier.Bronze, MessageKeys.BadgeFirstBudgetMet, MessageKeys.BadgeFirstBudgetMetDescription),

            [BadgeCode.IronDiscipline] = new(BadgeTier.Prata, MessageKeys.BadgeIronDiscipline, MessageKeys.BadgeIronDisciplineDescription),
            [BadgeCode.FlawlessQuarter] = new(BadgeTier.Prata, MessageKeys.BadgeFlawlessQuarter, MessageKeys.BadgeFlawlessQuarterDescription),
            [BadgeCode.SolidReserve] = new(BadgeTier.Prata, MessageKeys.BadgeSolidReserve, MessageKeys.BadgeSolidReserveDescription),
            [BadgeCode.ConsistentInvestor] = new(BadgeTier.Prata, MessageKeys.BadgeConsistentInvestor, MessageKeys.BadgeConsistentInvestorDescription),
            [BadgeCode.NoLatePayments] = new(BadgeTier.Prata, MessageKeys.BadgeNoLatePayments, MessageKeys.BadgeNoLatePaymentsDescription),
            [BadgeCode.QuarterInTheBlack] = new(BadgeTier.Prata, MessageKeys.BadgeQuarterInTheBlack, MessageKeys.BadgeQuarterInTheBlackDescription),
            [BadgeCode.DiligentPlanner] = new(BadgeTier.Prata, MessageKeys.BadgeDiligentPlanner, MessageKeys.BadgeDiligentPlannerDescription),
            [BadgeCode.DoubleGoalAchiever] = new(BadgeTier.Prata, MessageKeys.BadgeDoubleGoalAchiever, MessageKeys.BadgeDoubleGoalAchieverDescription),
            [BadgeCode.NoRelapses] = new(BadgeTier.Prata, MessageKeys.BadgeNoRelapses, MessageKeys.BadgeNoRelapsesDescription),
            [BadgeCode.QuickResponder] = new(BadgeTier.Prata, MessageKeys.BadgeQuickResponder, MessageKeys.BadgeQuickResponderDescription),
            [BadgeCode.TrendReverser] = new(BadgeTier.Prata, MessageKeys.BadgeTrendReverser, MessageKeys.BadgeTrendReverserDescription),
            [BadgeCode.WideBudgetCoverage] = new(BadgeTier.Prata, MessageKeys.BadgeWideBudgetCoverage, MessageKeys.BadgeWideBudgetCoverageDescription),
            [BadgeCode.RecoveredGoal] = new(BadgeTier.Prata, MessageKeys.BadgeRecoveredGoal, MessageKeys.BadgeRecoveredGoalDescription),
            [BadgeCode.SixMonthsFocused] = new(BadgeTier.Prata, MessageKeys.BadgeSixMonthsFocused, MessageKeys.BadgeSixMonthsFocusedDescription),

            [BadgeCode.PerfectYear] = new(BadgeTier.Ouro, MessageKeys.BadgePerfectYear, MessageKeys.BadgePerfectYearDescription),
            [BadgeCode.RobustReserve] = new(BadgeTier.Ouro, MessageKeys.BadgeRobustReserve, MessageKeys.BadgeRobustReserveDescription),
            [BadgeCode.DedicatedInvestor] = new(BadgeTier.Ouro, MessageKeys.BadgeDedicatedInvestor, MessageKeys.BadgeDedicatedInvestorDescription),
            [BadgeCode.PositiveFiscalYear] = new(BadgeTier.Ouro, MessageKeys.BadgePositiveFiscalYear, MessageKeys.BadgePositiveFiscalYearDescription),
            [BadgeCode.IronDisciplineIII] = new(BadgeTier.Ouro, MessageKeys.BadgeIronDisciplineIII, MessageKeys.BadgeIronDisciplineIIIDescription),
            [BadgeCode.GoldenStreak] = new(BadgeTier.Ouro, MessageKeys.BadgeGoldenStreak, MessageKeys.BadgeGoldenStreakDescription),
            [BadgeCode.AmbitiousGoalAchieved] = new(BadgeTier.Ouro, MessageKeys.BadgeAmbitiousGoalAchieved, MessageKeys.BadgeAmbitiousGoalAchievedDescription),
            [BadgeCode.NoLatePaymentsYear] = new(BadgeTier.Ouro, MessageKeys.BadgeNoLatePaymentsYear, MessageKeys.BadgeNoLatePaymentsYearDescription),
            [BadgeCode.PlanningMaster] = new(BadgeTier.Ouro, MessageKeys.BadgePlanningMaster, MessageKeys.BadgePlanningMasterDescription),
            [BadgeCode.Resilient] = new(BadgeTier.Ouro, MessageKeys.BadgeResilient, MessageKeys.BadgeResilientDescription),
            [BadgeCode.PlanningLegend] = new(BadgeTier.Ouro, MessageKeys.BadgePlanningLegend, MessageKeys.BadgePlanningLegendDescription),
            [BadgeCode.CompleteCollection] = new(BadgeTier.Ouro, MessageKeys.BadgeCompleteCollection, MessageKeys.BadgeCompleteCollectionDescription)
        };

    public static BadgeCatalogEntry Get(BadgeCode code) => Entries[code];

    public static IReadOnlyDictionary<BadgeCode, BadgeCatalogEntry> All => Entries;
}
