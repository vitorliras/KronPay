using Application.DTOs.Goals;
using FluentValidation;

namespace Application.Validators.Goals;

public sealed class CreateCategoryBudgetGoalRequestValidator : AbstractValidator<CreateCategoryBudgetGoalRequest>
{
    public CreateCategoryBudgetGoalRequestValidator()
    {
        RuleFor(x => x.CategoryId)
            .GreaterThan(0);

        RuleFor(x => x.MonthlyLimit)
            .GreaterThan(0);
    }
}
