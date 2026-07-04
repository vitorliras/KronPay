using Application.DTOs.Auth;
using FluentValidation;

namespace Application.Validators.Auth;

public sealed class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Code)
            .NotEmpty()
            .Length(6);

        RuleFor(x => x.NewPassword)
            .NotEmpty();

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty()
            .Equal(x => x.NewPassword);
    }
}
