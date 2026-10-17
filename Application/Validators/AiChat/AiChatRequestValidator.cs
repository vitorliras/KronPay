using Application.DTOs.AiChat;
using FluentValidation;
using Shared.Localization;

namespace Application.Validators.AiChat;

public sealed class AiChatRequestValidator : AbstractValidator<AiChatRequest>
{
    public const int MaxMessages = 50;
    public const int MaxMessageLength = 1000;

    public AiChatRequestValidator()
    {
        RuleFor(x => x.Messages)
            .NotEmpty().WithMessage(MessageKeys.InvalidDescription)
            .Must(m => m is null || m.Count <= MaxMessages).WithMessage(MessageKeys.TextTooLong)
            .Must(m => m is null || m.Count == 0 || IsRole(m[^1].Role, "user")).WithMessage(MessageKeys.InvalidDescription);

        RuleForEach(x => x.Messages)
            .ChildRules(message =>
            {
                message.RuleFor(m => m.Role)
                    .Must(role => IsRole(role, "user") || IsRole(role, "assistant"))
                    .WithMessage(MessageKeys.InvalidDescription);

                message.RuleFor(m => m.Content)
                    .NotEmpty().WithMessage(MessageKeys.InvalidDescription)
                    .MaximumLength(MaxMessageLength).WithMessage(MessageKeys.TextTooLong);
            });
    }

    private static bool IsRole(string? role, string expected) =>
        string.Equals(role?.Trim(), expected, StringComparison.OrdinalIgnoreCase);
}
