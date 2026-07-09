using Application.DTOs.Assistant;

namespace Application.Services.Assistant;

public sealed record TopicResolution(string? ResolvedNodeId, IReadOnlyList<AssistantOptionResponse>? ClarifyOptions)
{
    public static readonly TopicResolution None = new(null, null);

    public static TopicResolution Resolved(string nodeId) => new(nodeId, null);

    public static TopicResolution Ambiguous(IReadOnlyList<AssistantOptionResponse> options) => new(null, options);
}
