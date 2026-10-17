using System.Globalization;

namespace Application.Services.AiChat;

public static class MoneyFormatter
{
    public static string Format(decimal value)
    {
        var text = value.ToString("#,##0.00", CultureInfo.InvariantCulture);

        return "R$ " + text.Replace(",", "#").Replace(".", ",").Replace("#", ".");
    }
}
