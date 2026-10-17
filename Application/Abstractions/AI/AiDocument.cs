namespace Application.Abstractions.AI;

public sealed record AiDocument(AiAttachment? Attachment, IReadOnlyList<string> TableChunks);
