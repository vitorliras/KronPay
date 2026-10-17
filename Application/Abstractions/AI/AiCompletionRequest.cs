namespace Application.Abstractions.AI;

public sealed record AiCompletionRequest(
    int UserId,
    string Feature,
    string SystemPrompt,
    string UserPrompt,
    IReadOnlyList<AiAttachment>? Attachments = null);
