namespace Application.DTOs.AiChat;

public sealed record AiChatRequest(
    IReadOnlyList<AiChatMessageDto> Messages,
    IReadOnlyList<string>? AttachmentIds = null);
