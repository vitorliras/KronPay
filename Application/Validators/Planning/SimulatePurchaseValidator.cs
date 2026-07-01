using Application.DTOs.Planning;
using Application.Planning;
using FluentValidation;

namespace Application.Validators.Planning;

public sealed class SimulatePurchaseValidator : AbstractValidator<SimulatePurchaseRequest>
{
    public SimulatePurchaseValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0);

        RuleFor(x => x.PurchaseDate)
            .NotEmpty();

        When(x => x.Installment, () =>
        {
            RuleFor(x => x.InstallmentsCount)
                .GreaterThanOrEqualTo((short)1);
        });

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
