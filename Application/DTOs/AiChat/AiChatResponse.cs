namespace Application.DTOs.AiChat;

public sealed record AiChatResponse(
    string Reply,
    IReadOnlyList<string> Suggestions,
    IReadOnlyList<AiChatActionResponse> Actions);
