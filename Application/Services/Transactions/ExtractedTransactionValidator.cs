using System.Globalization;
using System.Text.RegularExpressions;
using Application.Abstractions.AI;

namespace Application.Services.Transactions;

public static class ExtractedTransactionValidator
{
    private const int MaxDescriptionLength = 300;
    private const int MaxYearsInThePast = 15;
    private const int MaxDaysInTheFuture = 366;

    private static readonly Regex Whitespace = new(@"\s+", RegexOptions.Compiled);

    public static IReadOnlyList<ExtractedTransaction> Validate(
        IEnumerable<ExtractedTransactionDraft> drafts,
        DateTime today)
    {
        var result = new List<ExtractedTransaction>();

        foreach (var draft in drafts)
        {
            if (!TryNormalize(draft.Date, draft.Description, draft.Amount, today, out var date, out var description, out var amount))
                continue;

            result.Add(new ExtractedTransaction(date, description, amount, NormalizeType(draft)));
        }

        return result;
    }

    public static bool TryNormalize(
        string? rawDate,
        string? rawDescription,
        decimal? rawAmount,
        DateTime today,
        out DateTime date,
        out string description,
        out decimal amount)
    {
        description = string.Empty;
        amount = 0m;

        if (!DateTime.TryParseExact(
                rawDate,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out date))
            return false;

        if (date < today.Date.AddYears(-MaxYearsInThePast) || date > today.Date.AddDays(MaxDaysInTheFuture))
            return false;

        description = CleanDescription(rawDescription);
        if (description.Length == 0)
            return false;

        if (rawAmount is null)
            return false;

        amount = Math.Round(Math.Abs(rawAmount.Value), 2);

        return amount > 0;
    }

    private static string CleanDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return string.Empty;

        var cleaned = Whitespace.Replace(description.Trim(), " ");

        return cleaned.Length <= MaxDescriptionLength ? cleaned : cleaned[..MaxDescriptionLength].TrimEnd();
    }

    private static string NormalizeType(ExtractedTransactionDraft draft)
    {
        var type = draft.Type?.Trim().ToUpperInvariant();

        return type is "I" or "R" ? "I" : "E";
    }
}
