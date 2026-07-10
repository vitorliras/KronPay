using Application.Services.Gamification;
using Domain.Entities.Transactions;
using Domain.Enums.Gamification;
using Domain.Interfaces.Gamification;
using Domain.Interfaces.Transactions;
using Domain.Services.Gamification;

namespace Application.Services.Gamification.Evaluators;

public sealed class TransactionMissionEvaluator : IMissionEvaluator
{
    private const int TrailingWindowMonths = 3;
    private const int FiscalYearWindowMonths = 12;
    private const int PersonalBestLookbackMonths = 12;
    private const int PositiveStreakMilestone = 6;
    private const decimal Investment20Threshold = 0.2m;
    private const decimal Investment10Threshold = 0.1m;
    private const decimal SpendingConcentrationThreshold = 0.4m;
    private const decimal SpendingDeclineThreshold = 0.85m;
    private const decimal SpendingSpikeThreshold = 1.15m;
    private const int IncomeDiversifiedMinCategories = 2;

    private const string PositiveMonthsStreakKey = "PositiveMonthsStreak";
    private const string InvestmentTwentyStreakKey = "InvestmentTwentyPlusStreak";
    private const string HasTransactionKey = "HasTransaction";
    private const string HasPositiveMonthKey = "HasPositiveMonth";
    private const string HasFullyCategorizedMonthKey = "HasFullyCategorizedMonth";
    private const string HasInvestmentKey = "HasInvestment";
    private const string HasFiscalYearPositiveKey = "HasFiscalYearPositive";
    private const string HasMonthSurplusKey = "HasMonthSurplus";

    private readonly ITransactionRepository _transactions;
    private readonly IConsistencyCounterRepository _counters;

    public TransactionMissionEvaluator(ITransactionRepository transactions, IConsistencyCounterRepository counters)
    {
        _transactions = transactions;
        _counters = counters;
    }

    public async Task<IReadOnlyList<MissionEvaluationResult>> EvaluateAsync(int userId, DateTime asOf)
    {
        var results = new List<MissionEvaluationResult>();

        var monthStart = new DateTime(asOf.Year, asOf.Month, 1);
        var trailingStart = monthStart.AddMonths(-(TrailingWindowMonths - 1));
        var fiscalStart = monthStart.AddMonths(-(FiscalYearWindowMonths - 1));
        var lookbackStart = monthStart.AddMonths(-(PersonalBestLookbackMonths - 1));

        var allRelevant = (await _transactions.GetByPeriodAsync(userId, lookbackStart, asOf))
            .Where(t => t.Status != "C")
            .ToList();

        if (allRelevant.Count > 0)
            await ConsistencyCounterUpdater.MarkOnceAsync(_counters, userId, HasTransactionKey);

        var trailing = allRelevant.Where(t => t.TransactionDate >= trailingStart).ToList();
        var trailingIncome = Sum(trailing, "I");
        var trailingExpense = Sum(trailing, "E");
        var trailingBalance = trailingIncome - trailingExpense;

        results.Add(new MissionEvaluationResult(MissionEventType.TransactionMonthSurplus, null, trailingBalance > 0));
        results.Add(new MissionEvaluationResult(MissionEventType.TransactionMonthDeficit, null, trailingBalance < 0));

        if (trailingBalance > 0)
            await ConsistencyCounterUpdater.MarkOnceAsync(_counters, userId, HasMonthSurplusKey);

        var trailingInvested = Sum(trailing, "V");
        var trailingRedeemed = Sum(trailing, "R");
        var investedRatio = trailingIncome > 0 ? trailingInvested / trailingIncome : 0m;
        var isTwentyPlus = investedRatio >= Investment20Threshold;
        var isTenToTwenty = investedRatio >= Investment10Threshold && investedRatio < Investment20Threshold;
        var isBelowTen = trailingIncome > 0 && investedRatio < Investment10Threshold;

        results.Add(new MissionEvaluationResult(MissionEventType.InvestmentTwentyPercentPlus, null, isTwentyPlus));
        results.Add(new MissionEvaluationResult(MissionEventType.InvestmentTenToTwentyPercent, null, isTenToTwenty));
        results.Add(new MissionEvaluationResult(MissionEventType.InvestmentBelowTenPercent, null, isBelowTen));
        results.Add(new MissionEvaluationResult(MissionEventType.InvestmentWithdrawal, null, trailingRedeemed > 0));

        if (trailingInvested > 0)
            await ConsistencyCounterUpdater.MarkOnceAsync(_counters, userId, HasInvestmentKey);

        await ConsistencyCounterUpdater.UpdateStreakAsync(_counters, userId, InvestmentTwentyStreakKey, isTwentyPlus);

        var fiscalWindow = allRelevant.Where(t => t.TransactionDate >= fiscalStart).ToList();
        var fiscalBalance = Sum(fiscalWindow, "I") - Sum(fiscalWindow, "E");
        results.Add(new MissionEvaluationResult(MissionEventType.FiscalYearPositive, null, fiscalBalance > 0));

        if (fiscalBalance > 0)
            await ConsistencyCounterUpdater.MarkOnceAsync(_counters, userId, HasFiscalYearPositiveKey);

        var monthlyBalances = new List<(int Year, int Month, decimal Income, decimal Expense, decimal Balance)>();
        for (var cursor = lookbackStart; cursor <= monthStart; cursor = cursor.AddMonths(1))
        {
            var monthTx = allRelevant
                .Where(t => t.TransactionDate.Year == cursor.Year && t.TransactionDate.Month == cursor.Month)
                .ToList();

            var income = Sum(monthTx, "I");
            var expense = Sum(monthTx, "E");
            monthlyBalances.Add((cursor.Year, cursor.Month, income, expense, income - expense));
        }

        var currentMonth = monthlyBalances[^1];
        var priorMonths = monthlyBalances.Take(monthlyBalances.Count - 1).ToList();

        var isFullyCategorized = IsFullyCategorized(allRelevant, monthStart, asOf);
        results.Add(new MissionEvaluationResult(MissionEventType.TransactionFullyCategorized, null, isFullyCategorized));
        results.Add(new MissionEvaluationResult(MissionEventType.TransactionUncategorizedPileup, null, !isFullyCategorized));

        if (isFullyCategorized)
            await ConsistencyCounterUpdater.MarkOnceAsync(_counters, userId, HasFullyCategorizedMonthKey);

        if (currentMonth.Balance > 0)
            await ConsistencyCounterUpdater.MarkOnceAsync(_counters, userId, HasPositiveMonthKey);

        var positiveStreak = await ConsistencyCounterUpdater.UpdateStreakAsync(
            _counters, userId, PositiveMonthsStreakKey, currentMonth.Balance > 0);
        results.Add(new MissionEvaluationResult(MissionEventType.TransactionPositiveStreak, null, positiveStreak >= PositiveStreakMilestone));

        if (priorMonths.Count > 0)
        {
            var bestPrior = priorMonths.Max(m => m.Balance);
            results.Add(new MissionEvaluationResult(MissionEventType.PersonalBestMonth, null, currentMonth.Balance > bestPrior));

            var avgPriorExpense = priorMonths.Average(m => m.Expense);
            var declined = avgPriorExpense > 0 && currentMonth.Expense <= avgPriorExpense * SpendingDeclineThreshold;
            var spiked = avgPriorExpense > 0 && currentMonth.Expense >= avgPriorExpense * SpendingSpikeThreshold;

            results.Add(new MissionEvaluationResult(MissionEventType.TotalSpendingDecline, null, declined));
            results.Add(new MissionEvaluationResult(MissionEventType.TotalSpendingSpike, null, spiked));
        }

        var currentMonthTx = allRelevant
            .Where(t => t.TransactionDate.Year == asOf.Year && t.TransactionDate.Month == asOf.Month)
            .ToList();

        var incomeCategoryCount = currentMonthTx
            .Where(t => t.CodTypeTransaction == "I" && t.CategoryId.HasValue)
            .Select(t => t.CategoryId!.Value)
            .Distinct()
            .Count();

        results.Add(new MissionEvaluationResult(MissionEventType.IncomeDiversified, null, incomeCategoryCount >= IncomeDiversifiedMinCategories));

        var currentExpenseTx = currentMonthTx.Where(t => t.CodTypeTransaction == "E").ToList();
        var totalExpense = currentExpenseTx.Sum(t => t.Amount);
        var isConcentrated = false;

        if (totalExpense > 0)
        {
            var byCategory = currentExpenseTx
                .Where(t => t.CategoryId.HasValue)
                .GroupBy(t => t.CategoryId!.Value)
                .Select(g => g.Sum(t => t.Amount));

            isConcentrated = byCategory.Any(amount => amount / totalExpense >= SpendingConcentrationThreshold);
        }

        results.Add(new MissionEvaluationResult(MissionEventType.SpendingConcentration, null, isConcentrated));

        return results;
    }

    private static decimal Sum(IEnumerable<Transaction> transactions, string codTypeTransaction)
        => transactions.Where(t => t.CodTypeTransaction == codTypeTransaction).Sum(t => t.Amount);

    private static bool IsFullyCategorized(IReadOnlyList<Transaction> all, DateTime monthStart, DateTime asOf)
    {
        var monthTransactions = all.Where(t => t.TransactionDate >= monthStart && t.TransactionDate <= asOf).ToList();
        return monthTransactions.Count > 0 && monthTransactions.All(t => t.CategoryId.HasValue);
    }
}
