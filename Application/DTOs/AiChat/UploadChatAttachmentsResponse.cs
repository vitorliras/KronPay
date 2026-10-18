namespace Application.DTOs.AiChat;

public sealed record UploadChatAttachmentsResponse(
    IReadOnlyList<ChatAttachmentSummaryResponse> Attachments,
    IReadOnlyList<string> UnreadableFiles);
