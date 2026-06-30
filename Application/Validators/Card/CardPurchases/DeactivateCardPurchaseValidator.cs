using Application.DTOs.Card.CardPurchases;
using FluentValidation;

namespace Application.Validators.Card.CardPurchases;

public sealed class DeactivateCardPurchaseValidator : AbstractValidator<DeactivateCardPurchaseRequest>
{
    public DeactivateCardPurchaseValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);
    }
}
