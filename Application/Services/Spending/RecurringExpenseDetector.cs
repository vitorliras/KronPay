using Domain.Services.Transactions;

namespace Application.Services.Spending;

public static class RecurringExpenseDetector
{
    private const int MinimumMonths = 3;
    private const decimal MaxDeviation = 0.25m;

    public static IReadOnlyList<RecurringExpense> Detect(IEnumerable<SpendingEntry> entries)
    {
        return entries
            .Select(e => (Entry: e, Key: TransactionDescriptionNormalizer.Normalize(e.Description)))
            .Where(x => x.Key.Length > 0)
            .GroupBy(x => x.Key)
            .Select(group => ToRecurring(group.Select(x => x.Entry).ToList()))
            .Where(r => r is not null)
            .Select(r => r!)
            .OrderByDescending(r => r.AverageAmount)
            .ToList();
    }

    private static RecurringExpense? ToRecurring(IReadOnlyList<SpendingEntry> entries)
    {
        var byMonth = entries
            .GroupBy(e => (e.Date.Year, e.Date.Month))
            .Select(g => (Month: g.Key, Total: g.Sum(e => e.Amount)))
            .ToList();

        if (byMonth.Count < MinimumMonths)
            return null;

        var average = byMonth.Average(m => m.Total);

        if (average <= 0 || byMonth.Any(m => Math.Abs(m.Total - average) / average > MaxDeviation))
            return null;

        var last = entries.OrderByDescending(e => e.Date).First();

        return new RecurringExpense(
            last.Description,
            byMonth.Count,
            Math.Round(average, 2),
            last.Date,
            last.CategoryId);
    }
}
