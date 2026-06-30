using Application.DTOs.Card.CardInvoices;
using FluentValidation;

namespace Application.Validators.Card.CardInvoices;

public sealed class PayCardInvoiceValidator : AbstractValidator<PayCardInvoiceRequest>
{
    public PayCardInvoiceValidator()
    {
        RuleFor(x => x.CardInvoiceId)
            .GreaterThan(0);

        RuleFor(x => x.PaymentMethodId)
            .GreaterThan(0);

        RuleFor(x => x.CodTypeTransaction)
            .NotEmpty()
            .MaximumLength(1);
    }
}
