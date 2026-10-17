using Application.DTOs.Transactions;
using FluentValidation;
using Shared.Localization;

namespace Application.Validators.Transactions;

public sealed class ParseTransactionsFromTextRequestValidator : AbstractValidator<ParseTransactionsFromTextRequest>
{
    public const int MaxTextLength = 4000;

    public ParseTransactionsFromTextRequestValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty().WithMessage(MessageKeys.InvalidDescription)
            .MaximumLength(MaxTextLength).WithMessage(MessageKeys.TextTooLong);
    }
}
