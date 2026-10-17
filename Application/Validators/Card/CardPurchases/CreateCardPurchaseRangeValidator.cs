using Application.DTOs.Card.CardPurchases;
using Application.Services.Card;
using FluentValidation;
using Shared.Localization;

namespace Application.Validators.Card.CardPurchases;

public sealed class CreateCardPurchaseRangeValidator : AbstractValidator<CreateCardPurchaseRangeRequest>
{
    public const int MaxPurchases = 200;

    public CreateCardPurchaseRangeValidator()
    {
        RuleFor(x => x.CreditCardId)
            .GreaterThan(0).WithMessage(MessageKeys.CreditCardNotFound);

        RuleFor(x => x.Purchases)
            .NotEmpty().WithMessage(MessageKeys.InvalidAmount)
            .Must(p => p is null || p.Count <= MaxPurchases).WithMessage(MessageKeys.FileTooLarge);

        RuleForEach(x => x.Purchases)
            .ChildRules(item =>
            {
                item.RuleFor(i => i.Description)
                    .NotEmpty().WithMessage(MessageKeys.InvalidDescription)
                    .MaximumLength(ExtractedCardPurchaseValidator.MaxDescriptionLength).WithMessage(MessageKeys.InvalidDescription);

                item.RuleFor(i => i.TotalAmount)
                    .GreaterThan(0).WithMessage(MessageKeys.InvalidAmount);

                item.RuleFor(i => i.InstallmentsCount)
                    .InclusiveBetween((short)1, (short)ExtractedCardPurchaseValidator.MaxInstallments)
                    .WithMessage(MessageKeys.InvalidInstallments);

                item.RuleFor(i => i.PurchaseDate)
                    .NotEqual(default(DateTime)).WithMessage(MessageKeys.InvalidDescription);
            });
    }
}
