using Application.DTOs.Card.CardPurchases;
using FluentValidation;

namespace Application.Validators.Card.CardPurchases;

public sealed class UpdateCardPurchaseValidator : AbstractValidator<UpdateCardPurchaseRequest>
{
    public UpdateCardPurchaseValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(60);
    }
}
