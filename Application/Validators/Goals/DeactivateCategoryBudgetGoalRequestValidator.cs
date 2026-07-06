using Application.DTOs.Goals;
using FluentValidation;

namespace Application.Validators.Goals;

public sealed class DeactivateCategoryBudgetGoalRequestValidator : AbstractValidator<DeactivateCategoryBudgetGoalRequest>
{
    public DeactivateCategoryBudgetGoalRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);
    }
}
