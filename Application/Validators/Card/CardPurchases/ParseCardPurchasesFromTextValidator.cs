using Application.DTOs.Card.CardPurchases;
using Application.Validators.Transactions;
using FluentValidation;
using Shared.Localization;

namespace Application.Validators.Card.CardPurchases;

public sealed class ParseCardPurchasesFromTextValidator : AbstractValidator<ParseCardPurchasesFromTextRequest>
{
    public ParseCardPurchasesFromTextValidator()
    {
        RuleFor(x => x.CreditCardId)
            .GreaterThan(0).WithMessage(MessageKeys.CreditCardNotFound);

        RuleFor(x => x.Text)
            .NotEmpty().WithMessage(MessageKeys.InvalidDescription)
            .MaximumLength(ParseTransactionsFromTextRequestValidator.MaxTextLength).WithMessage(MessageKeys.TextTooLong);
    }
}
