using Application.DTOs.Configuration.Categories;
using FluentValidation;

namespace Application.Validators.Configuration.Category;

public sealed class DeactivateCategoryValidator
    : AbstractValidator<DeactivateCategoryRequest>
{
    public DeactivateCategoryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);
    }
}
