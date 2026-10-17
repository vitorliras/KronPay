using System.Globalization;
using System.Text;

namespace Domain.Services.Transactions;

public static class TransactionDescriptionNormalizer
{
    private static readonly HashSet<string> MonthNames = new(StringComparer.Ordinal)
    {
        "janeiro", "fevereiro", "marco", "abril", "maio", "junho",
        "julho", "agosto", "setembro", "outubro", "novembro", "dezembro"
    };

    public static string Normalize(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return string.Empty;

        var decomposed = description.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);

        var letters = new StringBuilder(decomposed.Length);

        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
                continue;

            letters.Append(char.IsLetter(character) ? character : ' ');
        }

        var tokens = letters
            .ToString()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(token => !MonthNames.Contains(token));

        return string.Join(' ', tokens);
    }

    public static bool IsOutflow(string transactionType) =>
        transactionType is "E" or "V";
}
