namespace Application.Abstractions.AI;

public sealed record ExtractedCardPurchase(
    DateTime Date,
    string Description,
    decimal Amount,
    short InstallmentsCount);
