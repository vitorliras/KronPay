namespace Infrastructure.AI;

internal static class ExtractionSupport
{
    public static string? ExtractJsonArray(string text)
    {
        var start = text.IndexOf('[');
        var end = text.LastIndexOf(']');

        return start >= 0 && end > start ? text[start..(end + 1)] : null;
    }

    public static DateTime TodayInSaoPaulo() => Application.Services.Spending.BrazilClock.Today();
}
