using Application.DTOs.Auth;
using FluentValidation;

namespace Application.Validators.Auth;

public sealed class ResendCodeRequestValidator : AbstractValidator<ResendCodeRequest>
{
    public ResendCodeRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
    }
}
