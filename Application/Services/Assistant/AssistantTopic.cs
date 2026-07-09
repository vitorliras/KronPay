namespace Application.Services.Assistant;

public sealed record AssistantTopic(string NodeId, string LabelKey, IReadOnlyCollection<string> Phrases);
