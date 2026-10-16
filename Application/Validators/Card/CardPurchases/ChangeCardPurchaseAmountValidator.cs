using Application.DTOs.Card.CardPurchases;
using FluentValidation;

namespace Application.Validators.Card.CardPurchases;

public sealed class ChangeCardPurchaseAmountValidator : AbstractValidator<ChangeCardPurchaseAmountRequest>
{
    public ChangeCardPurchaseAmountValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);

        RuleFor(x => x.InstallmentAmount)
            .GreaterThanOrEqualTo(0.01m);
    }
}
