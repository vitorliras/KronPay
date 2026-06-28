namespace Application.DTOs.Configuration.CreditCards;

public sealed record CreateCreditCardRequest(
    string Description,
    short DueDay,
    short ClosingDay,
    int BankId,
    decimal CreditLimit
);

