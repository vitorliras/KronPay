namespace Application.DTOs.Configuration.CreditCards;

public sealed record CreateCreditCardRequest(
    int UserId,
    string Description,
    short DueDay,
    short ClosingDay,
    decimal CreditLimit
);

