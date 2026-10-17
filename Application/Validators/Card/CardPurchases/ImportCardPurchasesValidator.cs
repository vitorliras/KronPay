using Application.DTOs.Card.CardPurchases;
using FluentValidation;
using Shared.Localization;

namespace Application.Validators.Card.CardPurchases;

public sealed class ImportCardPurchasesValidator : AbstractValidator<ImportCardPurchasesRequest>
{
    public ImportCardPurchasesValidator()
    {
        RuleFor(x => x.CreditCardId)
            .GreaterThan(0).WithMessage(MessageKeys.CreditCardNotFound);

        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage(MessageKeys.UnsupportedFileFormat);
    }
}
