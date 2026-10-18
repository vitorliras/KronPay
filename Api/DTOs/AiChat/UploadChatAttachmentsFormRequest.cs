namespace Api.DTOs.AiChat;

public sealed record UploadChatAttachmentsFormRequest(List<IFormFile> Files);
