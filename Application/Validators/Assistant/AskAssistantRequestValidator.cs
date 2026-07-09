using Application.DTOs.Assistant;
using FluentValidation;
using Shared.Localization;

namespace Application.Validators.Assistant;

public sealed class AskAssistantRequestValidator : AbstractValidator<AskAssistantRequest>
{
    public AskAssistantRequestValidator()
    {
        RuleFor(x => x)
            .Must(x => string.IsNullOrWhiteSpace(x.SelectedOptionId) || string.IsNullOrWhiteSpace(x.FreeText))
            .WithMessage(MessageKeys.AssistantInvalidRequest);
    }
}
