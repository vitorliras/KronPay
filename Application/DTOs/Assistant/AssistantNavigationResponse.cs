namespace Application.DTOs.Assistant;

public sealed record AssistantNavigationResponse(
    string Path,
    IReadOnlyDictionary<string, string> QueryParams,
    bool AutoNavigate,
    string LabelKey);
