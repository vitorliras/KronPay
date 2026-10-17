namespace Application.Services.AiChat;

public sealed record ParsedChatReply(string Reply, IReadOnlyList<string> Suggestions);
