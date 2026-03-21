using Application.Abstractions.Import;
using Domain.Entities.Configuration;
using System.Globalization;
using System.Text.RegularExpressions;

public sealed class OfxTransactionImportParser : ITransactionImportParser
{
    public bool CanParse(string fileName)
        => fileName.EndsWith(".ofx", StringComparison.OrdinalIgnoreCase);

    public async Task<IEnumerable<ImportedTransactionResponse>> ParseAsync(
        Stream fileStream,
        int userId,
        IEnumerable<PaymentMethod>? paymentMethods = null,
        IEnumerable<Category>? categories = null)
    {
        using var reader = new StreamReader(fileStream);
        var content = await reader.ReadToEndAsync();

        var result = new List<ImportedTransactionResponse>();

        var transactions = Regex.Matches(content, "<STMTTRN>(.*?)</STMTTRN>", RegexOptions.Singleline);

        foreach (Match match in transactions)
        {
            var block = match.Value;

            var date = ParseDate(block);
            var amount = ParseDecimal(block, "TRNAMT");
            var description = NormalizeDescription(ParseString(block, "NAME"));
            if(string.IsNullOrEmpty(description))
                description = NormalizeDescription(ParseString(block, "MEMO"));

            result.Add(new ImportedTransactionResponse(
                date,
                Math.Abs(amount),
                description,
                amount < 0 ? "E" : "I",
                "P",
                1,
                null,
                null,
                null,
                null
            ));
        }

        return result;
    }

    private static DateTime ParseDate(string text)
    {
        var value = Regex.Match(text, "<DTPOSTED>(\\d{8})").Groups[1].Value;
        return DateTime.ParseExact(value, "yyyyMMdd", null);
    }

    private static decimal ParseDecimal(string text, string tag)
    {
        var value = Regex.Match(text, $"<{tag}>([-0-9.]+)").Groups[1].Value;
        return decimal.Parse(value, CultureInfo.InvariantCulture);
    }

    private static string ParseString(string text, string tag)
    {
        return Regex.Match(text, $"<{tag}>(.+)").Groups[1].Value.Trim();
    }

    private static string NormalizeDescription(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return string.Empty;

        return raw
            .Replace("</NAME>", "", StringComparison.OrdinalIgnoreCase)
            .Replace("</NAME>", "", StringComparison.OrdinalIgnoreCase)
            .Replace("</MEMO>", "", StringComparison.OrdinalIgnoreCase)
            .Replace("<MEMO>", "", StringComparison.OrdinalIgnoreCase)
            .Trim();
    }

}
