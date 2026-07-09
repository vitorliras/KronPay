namespace Application.DTOs.Assistant;

public sealed record AssistantNodeResponse(
    string NodeId,
    string MessageKey,
    IReadOnlyCollection<string> MessageArgs,
    IReadOnlyCollection<AssistantOptionResponse> Options,
    bool IsFinal,
    IReadOnlyCollection<AssistantNavigationResponse>? NavigateTo = null);
