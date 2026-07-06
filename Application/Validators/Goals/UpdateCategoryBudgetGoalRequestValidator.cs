using Application.DTOs.Goals;
using FluentValidation;

namespace Application.Validators.Goals;

public sealed class UpdateCategoryBudgetGoalRequestValidator : AbstractValidator<UpdateCategoryBudgetGoalRequest>
{
    public UpdateCategoryBudgetGoalRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);

        RuleFor(x => x.MonthlyLimit)
            .GreaterThan(0);
    }
}
