using Application.DTOs.Goals;
using FluentValidation;

namespace Application.Validators.Goals;

public sealed class GetCategoryBudgetGoalStrategyRequestValidator : AbstractValidator<GetCategoryBudgetGoalStrategyRequest>
{
    public GetCategoryBudgetGoalStrategyRequestValidator()
    {
        RuleFor(x => x.GoalId)
            .GreaterThan(0);
    }
}
