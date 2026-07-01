using Application.DTOs.Planning;
using FluentValidation;

namespace Application.Validators.Planning;

public sealed class DeactivatePlannedCommitmentValidator : AbstractValidator<DeactivatePlannedCommitmentRequest>
{
    public DeactivatePlannedCommitmentValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);
    }
}
