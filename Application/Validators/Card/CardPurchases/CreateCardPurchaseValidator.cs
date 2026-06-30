using Application.DTOs.Card.CardPurchases;
using FluentValidation;

namespace Application.Validators.Card.CardPurchases;

public sealed class CreateCardPurchaseValidator : AbstractValidator<CreateCardPurchaseRequest>
{
    public CreateCardPurchaseValidator()
    {
        RuleFor(x => x.CreditCardId)
            .GreaterThan(0);

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(60);

        RuleFor(x => x.TotalAmount)
            .GreaterThan(0);

        RuleFor(x => x.InstallmentsCount)
            .GreaterThanOrEqualTo((short)1);
    }
}
