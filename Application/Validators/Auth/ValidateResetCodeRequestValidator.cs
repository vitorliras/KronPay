using Application.DTOs.Auth;
using FluentValidation;

namespace Application.Validators.Auth;

public sealed class ValidateResetCodeRequestValidator : AbstractValidator<ValidateResetCodeRequest>
{
    public ValidateResetCodeRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Code)
            .NotEmpty()
            .Length(6);
    }
}
