using Domain.Exceptions;
using Shared.Localization;

namespace KronPay.Domain.Entities.Users;

public sealed class UserProfilePhoto
{
    private const long MaxPhotoSizeBytes = 350 * 1024;
    private static readonly string[] AllowedContentTypes = { "image/jpeg", "image/png", "image/webp" };

    public int Id { get; private set; }
    public int UserId { get; private set; }
    public byte[] Photo { get; private set; } = default!;
    public string ContentType { get; private set; } = default!;
    public DateTime UpdatedAt { get; private set; }

    protected UserProfilePhoto() { }

    public UserProfilePhoto(int userId, byte[] photo, string contentType)
    {
        if (userId <= 0)
            throw new DomainException(MessageKeys.InvaldUser);

        Validate(photo, contentType);

        UserId = userId;
        Photo = photo;
        ContentType = contentType;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Replace(byte[] photo, string contentType)
    {
        Validate(photo, contentType);

        Photo = photo;
        ContentType = contentType;
        UpdatedAt = DateTime.UtcNow;
    }

    private static void Validate(byte[] photo, string contentType)
    {
        if (photo is null || photo.Length == 0)
            throw new DomainException(MessageKeys.InvalidImageFormat);

        if (photo.Length > MaxPhotoSizeBytes)
            throw new DomainException(MessageKeys.ImageTooLarge);

        if (!AllowedContentTypes.Contains(contentType))
            throw new DomainException(MessageKeys.InvalidImageFormat);
    }
}
