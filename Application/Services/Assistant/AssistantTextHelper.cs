namespace Application.Services.Assistant;

public static class AssistantTextHelper
{
    public static string Truncate(string text, int maxLength) =>
        text.Length <= maxLength ? text : text[..maxLength].TrimEnd() + "...";
}
