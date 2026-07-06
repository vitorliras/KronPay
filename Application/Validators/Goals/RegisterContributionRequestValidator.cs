using Application.DTOs.Goals;
using FluentValidation;

namespace Application.Validators.Goals;

public sealed class RegisterContributionRequestValidator : AbstractValidator<RegisterContributionRequest>
{
    public RegisterContributionRequestValidator()
    {
        RuleFor(x => x.GoalId)
            .GreaterThan(0);

        RuleFor(x => x.Amount)
            .GreaterThan(0);
    }
}
