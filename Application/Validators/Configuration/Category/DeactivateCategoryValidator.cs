using Application.DTOs.Configuration.Categories;
using FluentValidation;

namespace Application.Validations.Categories;

public sealed class DeactivateCategoryValidator
    : AbstractValidator<DeactivateCategoryRequest>
{
    public DeactivateCategoryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);
    }
}
