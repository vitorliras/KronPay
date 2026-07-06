using Application.DTOs.Goals;
using FluentValidation;

namespace Application.Validators.Goals;

public sealed class SimulateExtraContributionRequestValidator : AbstractValidator<SimulateExtraContributionRequest>
{
    public SimulateExtraContributionRequestValidator()
    {
        RuleFor(x => x.GoalId)
            .GreaterThan(0);

        RuleFor(x => x.ExtraMonthlyAmount)
            .GreaterThan(0);
    }
}
