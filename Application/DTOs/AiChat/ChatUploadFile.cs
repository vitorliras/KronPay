namespace Application.DTOs.AiChat;

public sealed record ChatUploadFile(string FileName, long Length, Stream Content);
