using Application.DTOs.Notifications;
using FluentValidation;

namespace Application.Validators.Notifications;

public sealed class MarkNotificationAsReadRequestValidator : AbstractValidator<MarkNotificationAsReadRequest>
{
    public MarkNotificationAsReadRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);
    }
}
