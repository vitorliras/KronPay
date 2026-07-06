using Application.DTOs.Goals;
using FluentValidation;

namespace Application.Validators.Goals;

public sealed class RetryFinancialGoalRequestValidator : AbstractValidator<RetryFinancialGoalRequest>
{
    public RetryFinancialGoalRequestValidator()
    {
        RuleFor(x => x.GoalId)
            .GreaterThan(0);

        RuleFor(x => x.NewTargetDate)
            .GreaterThan(DateTime.UtcNow.Date);
    }
}
