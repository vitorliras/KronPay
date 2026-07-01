using Application.DTOs.Planning;
using Application.Planning;
using FluentValidation;

namespace Application.Validators.Planning;

public sealed class GetFinancialProjectionValidator : AbstractValidator<GetFinancialProjectionRequest>
{
    public GetFinancialProjectionValidator()
    {
        When(x => x.HorizonMonths.HasValue, () =>
        {
            RuleFor(x => x.HorizonMonths!.Value)
                .InclusiveBetween(1, PlanningDefaults.MaxHorizonMonths);
        });

        When(x => x.SafetyReserve.HasValue, () =>
        {
            RuleFor(x => x.SafetyReserve!.Value)
                .GreaterThanOrEqualTo(0);
        });
    }
}
