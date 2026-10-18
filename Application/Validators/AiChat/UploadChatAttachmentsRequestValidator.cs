using Application.Configuration.AI;
using Application.DTOs.AiChat;
using FluentValidation;
using Microsoft.Extensions.Options;
using Shared.Localization;

namespace Application.Validators.AiChat;

public sealed class UploadChatAttachmentsRequestValidator : AbstractValidator<UploadChatAttachmentsRequest>
{
    public UploadChatAttachmentsRequestValidator(IOptionsMonitor<AiSettings> settings)
    {
        RuleFor(x => x.Files)
            .NotEmpty().WithMessage(MessageKeys.UnsupportedFileFormat)
            .Must(files => files is null || files.Count <= settings.CurrentValue.Chat.MaxAttachmentsPerMessage)
            .WithMessage(MessageKeys.ChatAttachmentTooMany);

        RuleForEach(x => x.Files)
            .ChildRules(file =>
            {
                file.RuleFor(f => f.FileName).NotEmpty().WithMessage(MessageKeys.UnsupportedFileFormat);

                file.RuleFor(f => f.Length)
                    .GreaterThan(0).WithMessage(MessageKeys.UnsupportedFileFormat)
                    .Must(length => length <= settings.CurrentValue.Chat.MaxAttachmentBytes)
                    .WithMessage(MessageKeys.ChatAttachmentTooLarge);
            });
    }
}
