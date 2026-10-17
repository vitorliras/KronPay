using System.Text.Json;
using System.Text.RegularExpressions;

namespace Application.Services.AiChat;

public static class AiChatReplyParser
{
    public const int MaxReplyLength = 4000;
    public const int MaxSuggestions = 4;
    public const int MaxSuggestionLength = 80;

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private static readonly Regex PlaceholderMarker = new(@"\[\[[^\]]*\]\]", RegexOptions.Compiled);

    private static readonly Regex SuggestionsFragment = new(@"\{\s*""suggestions""\s*:\s*\[[^\]]*\]\s*\}", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex TrailingEmptyArray = new(@"(\[\s*\]\s*)+$", RegexOptions.Compiled);

    public static ParsedChatReply Parse(string? text)
    {
        var raw = StripFences((text ?? string.Empty).Trim());

        if (raw.Length == 0)
            return new ParsedChatReply(string.Empty, Array.Empty<string>());

        var json = ExtractJsonObject(raw);

        if (json is not null && TryDeserialize(json) is { } parsed && !string.IsNullOrWhiteSpace(parsed.Reply))
        {
            return new ParsedChatReply(
                Truncate(RemovePlaceholders(parsed.Reply), MaxReplyLength),
                CleanSuggestions(parsed.Suggestions));
        }

        var fragment = SuggestionsFragment.Match(raw);

        if (fragment.Success && TryDeserialize(fragment.Value) is { } extra)
        {
            var withoutFragment = raw.Remove(fragment.Index, fragment.Length);

            return new ParsedChatReply(
                Truncate(RemovePlaceholders(TrailingEmptyArray.Replace(withoutFragment.Trim(), string.Empty)), MaxReplyLength),
                CleanSuggestions(extra.Suggestions));
        }

        return new ParsedChatReply(Truncate(RemovePlaceholders(raw), MaxReplyLength), Array.Empty<string>());
    }

    private static RawReply? TryDeserialize(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<RawReply>(json, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string? ExtractJsonObject(string text)
    {
        var start = text.IndexOf('{');
        var end = text.LastIndexOf('}');

        return start >= 0 && end > start ? text[start..(end + 1)] : null;
    }

    private static IReadOnlyList<string> CleanSuggestions(IEnumerable<string>? suggestions) =>
        (suggestions ?? Array.Empty<string>())
            .Select(s => s?.Trim() ?? string.Empty)
            .Where(s => s.Length > 0)
            .Select(s => Truncate(s, MaxSuggestionLength))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(MaxSuggestions)
            .ToList();

    private static string RemovePlaceholders(string text) =>
        PlaceholderMarker.Replace(text, string.Empty).Trim();

    private static string StripFences(string text) =>
        text.Replace("```json", string.Empty, StringComparison.OrdinalIgnoreCase).Replace("```", string.Empty).Trim();

    private static string Truncate(string text, int maxLength) =>
        text.Length <= maxLength ? text : text[..maxLength].TrimEnd();

    private sealed record RawReply(string? Reply, List<string>? Suggestions);
}
