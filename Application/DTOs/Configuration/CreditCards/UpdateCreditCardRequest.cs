namespace Application.DTOs.Configuration.CreditCards;

public sealed record UpdateCreditCardRequest(
    int Id,
    string Description,
    int BankId,
    short DueDay,
    short ClosingDay,
    decimal CreditLimit
);
