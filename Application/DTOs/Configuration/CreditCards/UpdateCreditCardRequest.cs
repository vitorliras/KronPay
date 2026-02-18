namespace Application.DTOs.Configuration.CreditCards;

public sealed record UpdateCreditCardRequest(
    int Id,
    string Description,
    short DueDay,
    short ClosingDay,
    decimal CreditLimit
);
