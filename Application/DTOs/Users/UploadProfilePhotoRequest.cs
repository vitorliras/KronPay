namespace Application.DTOs.Users;

public sealed record UploadProfilePhotoRequest(byte[] Content, string ContentType);
