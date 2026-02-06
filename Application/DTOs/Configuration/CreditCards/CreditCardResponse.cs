namespace Application.DTOs.Configuration.CreditCards;

public sealed record CreditCardResponse(
    int Id,
    string Description,
    short DueDay,
    short ClosingDay,
    decimal CreditLimit,
    bool Active
);