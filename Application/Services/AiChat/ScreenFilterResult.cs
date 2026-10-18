namespace Application.Services.AiChat;

public sealed record ScreenFilterResult(
    IReadOnlyDictionary<string, string> QueryParams,
    IReadOnlyList<string> Applied,
    IReadOnlyList<string> Ignored);
