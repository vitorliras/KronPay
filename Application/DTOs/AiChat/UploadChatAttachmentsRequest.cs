namespace Application.DTOs.AiChat;

public sealed record UploadChatAttachmentsRequest(IReadOnlyList<ChatUploadFile> Files);
