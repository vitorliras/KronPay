using Application.DTOs.Goals;
using FluentValidation;

namespace Application.Validators.Goals;

public sealed class GetFinancialGoalDetailRequestValidator : AbstractValidator<GetFinancialGoalDetailRequest>
{
    public GetFinancialGoalDetailRequestValidator()
    {
        RuleFor(x => x.GoalId)
            .GreaterThan(0);
    }
}
