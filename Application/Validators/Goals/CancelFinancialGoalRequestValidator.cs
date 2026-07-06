using Application.DTOs.Goals;
using FluentValidation;

namespace Application.Validators.Goals;

public sealed class CancelFinancialGoalRequestValidator : AbstractValidator<CancelFinancialGoalRequest>
{
    public CancelFinancialGoalRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);
    }
}
