using Application.DTOs.Auth;
using FluentValidation;

namespace Application.Validators.Auth;

public sealed class RequestPasswordResetRequestValidator : AbstractValidator<RequestPasswordResetRequest>
{
    public RequestPasswordResetRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
    }
}
