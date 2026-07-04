using Application.DTOs.Users;
using FluentValidation;

namespace Application.Validators.Users;

public sealed class UploadProfilePhotoRequestValidator : AbstractValidator<UploadProfilePhotoRequest>
{
    private const long MaxUploadSizeBytes = 5 * 1024 * 1024;
    private static readonly string[] AllowedContentTypes = { "image/jpeg", "image/png", "image/webp" };

    public UploadProfilePhotoRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .Must(content => content.Length <= MaxUploadSizeBytes);

        RuleFor(x => x.ContentType)
            .NotEmpty()
            .Must(contentType => AllowedContentTypes.Contains(contentType));
    }
}
