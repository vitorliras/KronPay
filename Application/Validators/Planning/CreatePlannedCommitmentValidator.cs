using Application.DTOs.Planning;
using FluentValidation;

namespace Application.Validators.Planning;

public sealed class CreatePlannedCommitmentValidator : AbstractValidator<CreatePlannedCommitmentRequest>
{
    public CreatePlannedCommitmentValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(60);

        RuleFor(x => x.Amount)
            .GreaterThan(0);

        RuleFor(x => x.Direction)
            .Must(v => v is "I" or "O");

        RuleFor(x => x.Periodicity)
            .Must(v => v is "U" or "M" or "S" or "A");

        When(x => x.EndDate.HasValue, () =>
        {
            RuleFor(x => x.EndDate!.Value)
                .GreaterThanOrEqualTo(x => x.StartDate);
        });
    }
}
