namespace Application.DTOs.Configuration.CreditCards;

public sealed record UpdateCreditCardRequest(
    int Id,
    int UserId,
    string Description,
    short DueDay,
    short ClosingDay,
    decimal CreditLimit
);
