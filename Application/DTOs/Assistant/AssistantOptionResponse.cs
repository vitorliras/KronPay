namespace Application.DTOs.Assistant;

public sealed record AssistantOptionResponse(string Id, string LabelKey, IReadOnlyCollection<string> LabelArgs);
