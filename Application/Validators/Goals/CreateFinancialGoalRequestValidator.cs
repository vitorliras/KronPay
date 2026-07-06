using Application.DTOs.Goals;
using FluentValidation;

namespace Application.Validators.Goals;

public sealed class CreateFinancialGoalRequestValidator : AbstractValidator<CreateFinancialGoalRequest>
{
    public CreateFinancialGoalRequestValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.TargetAmount)
            .GreaterThan(0);

        RuleFor(x => x.TargetDate)
            .GreaterThan(DateTime.UtcNow.Date);
    }
}
