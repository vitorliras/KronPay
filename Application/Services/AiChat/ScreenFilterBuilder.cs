using System.Globalization;

namespace Application.Services.AiChat;

public sealed class ScreenFilterBuilder
{
    private const int MaxRangeDays = 366;
    private const int MaxSearchLength = 60;

    private static readonly string[] ReportTabs = { "expense", "income", "investment", "comparison", "card" };
    private static readonly string[] ConfigTabs = { "categories", "subcategories", "payments", "ai" };
    private static readonly int[] PlanningHorizons = { 6, 12, 24 };
    private static readonly string[] TransactionTypes = { "E", "I", "V" };

    private static readonly IReadOnlyDictionary<string, string[]> Allowed = new Dictionary<string, string[]>
    {
        ["dashboard"] = new[] { "month", "year" },
        ["transactions"] = new[] { "month", "year", "type", "categoryId", "search" },
        ["reports"] = new[] { "tab", "startDate", "endDate" },
        ["planning"] = new[] { "horizonMonths" },
        ["goals"] = new[] { "goalId" },
        ["goals-history"] = new[] { "search" },
        ["invoices"] = new[] { "cardId", "year", "month" },
        ["config"] = new[] { "tab", "type", "categoryId" }
    };

    public static bool Supports(string screenId) => Allowed.ContainsKey(screenId);

    public ScreenFilterResult Build(string screenId, ScreenFilterInput input, DateTime today)
    {
        var query = new Dictionary<string, string>();
        var applied = new List<string>();
        var ignored = new List<string>();

        if (!Allowed.TryGetValue(screenId, out var allowed))
        {
            AddIgnoredIfProvided(input, ignored, "this screen has no filters");
            return new ScreenFilterResult(query, applied, ignored);
        }

        void Apply(string key, string value, string description)
        {
            query[key] = value;
            applied.Add(description);
        }

        if (input.Tab is not null)
        {
            var tab = input.Tab.Trim().ToLowerInvariant();
            var valid = screenId == "reports" ? ReportTabs : screenId == "config" ? ConfigTabs : Array.Empty<string>();

            if (allowed.Contains("tab") && valid.Contains(tab))
                Apply("tab", tab, $"tab={tab}");
            else
                ignored.Add($"tab '{input.Tab}'");
        }

        ApplyPeriod(screenId, input, today, allowed, Apply, ignored);
        ApplyMonthYear(input, today, allowed, Apply, ignored);

        if (input.Type is not null)
        {
            var type = input.Type.Trim().ToUpperInvariant();

            if (allowed.Contains("type") && TransactionTypes.Contains(type))
                Apply("type", type, $"type={type}");
            else
                ignored.Add($"type '{input.Type}'");
        }

        if (input.CategoryId is { } categoryId)
        {
            if (allowed.Contains("categoryId") && categoryId > 0)
                Apply("categoryId", categoryId.ToString(CultureInfo.InvariantCulture), "category");
            else
                ignored.Add("category");
        }

        if (!string.IsNullOrWhiteSpace(input.Search))
        {
            var search = input.Search.Trim();

            if (allowed.Contains("search") && search.Length <= MaxSearchLength)
                Apply("search", search, $"search='{search}'");
            else
                ignored.Add("search");
        }

        if (input.HorizonMonths is { } horizon)
        {
            if (allowed.Contains("horizonMonths") && PlanningHorizons.Contains(horizon))
                Apply("horizonMonths", horizon.ToString(CultureInfo.InvariantCulture), $"horizon={horizon} months");
            else
                ignored.Add($"horizon {horizon} (valid: 6, 12, 24)");
        }

        if (input.CardId is { } cardId)
        {
            if (allowed.Contains("cardId") && cardId > 0)
                Apply("cardId", cardId.ToString(CultureInfo.InvariantCulture), "card");
            else
                ignored.Add("card");
        }

        if (input.GoalId is { } goalId)
        {
            if (allowed.Contains("goalId") && goalId > 0)
                Apply("goalId", goalId.ToString(CultureInfo.InvariantCulture), "goal");
            else
                ignored.Add("goal");
        }

        return new ScreenFilterResult(query, applied, ignored);
    }

    private static void ApplyPeriod(
        string screenId,
        ScreenFilterInput input,
        DateTime today,
        string[] allowed,
        Action<string, string, string> apply,
        List<string> ignored)
    {
        if (input.StartDate is null && input.EndDate is null)
            return;

        if (!allowed.Contains("startDate"))
        {
            ignored.Add("dates");
            return;
        }

        if (input.StartDate is null)
        {
            ignored.Add("end date without a start date");
            return;
        }

        var start = input.StartDate.Value.Date;
        var end = (input.EndDate ?? today).Date;

        if (start > end)
        {
            ignored.Add("dates (start is after the end)");
            return;
        }

        if ((end - start).TotalDays > MaxRangeDays)
        {
            ignored.Add($"dates (range longer than {MaxRangeDays} days)");
            return;
        }

        if (end > today.AddYears(1))
        {
            ignored.Add("dates (too far in the future)");
            return;
        }

        apply("startDate", Iso(start), $"from {Iso(start)}");
        apply("endDate", Iso(end), $"to {Iso(end)}");
    }

    private static void ApplyMonthYear(
        ScreenFilterInput input,
        DateTime today,
        string[] allowed,
        Action<string, string, string> apply,
        List<string> ignored)
    {
        if (input.Month is null && input.Year is null)
            return;

        if (!allowed.Contains("month") && !allowed.Contains("year"))
        {
            ignored.Add("month/year");
            return;
        }

        if (input.Month is { } month && (month < 1 || month > 12))
        {
            ignored.Add($"month {month}");
            return;
        }

        if (input.Year is { } year && (year < 2000 || year > today.Year + 1))
        {
            ignored.Add($"year {year}");
            return;
        }

        if (allowed.Contains("month") && input.Month is { } validMonth)
            apply("month", validMonth.ToString(CultureInfo.InvariantCulture), $"month={validMonth}");

        if (allowed.Contains("year") && (input.Year ?? (input.Month is null ? null : today.Year)) is { } validYear)
            apply("year", validYear.ToString(CultureInfo.InvariantCulture), $"year={validYear}");
    }

    private static void AddIgnoredIfProvided(ScreenFilterInput input, List<string> ignored, string reason)
    {
        if (input != new ScreenFilterInput())
            ignored.Add(reason);
    }

    private static string Iso(DateTime date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}
