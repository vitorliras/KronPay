using Application.DTOs.Goals;
using FluentValidation;

namespace Application.Validators.Goals;

public sealed class UpdateFinancialGoalRequestValidator : AbstractValidator<UpdateFinancialGoalRequest>
{
    public UpdateFinancialGoalRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.TargetAmount)
            .GreaterThan(0);

        RuleFor(x => x.TargetDate)
            .GreaterThan(DateTime.UtcNow.Date);
    }
}
