namespace Application.Abstractions.AI;

public sealed record ExtractedCardPurchaseDraft(
    string? Date,
    string? Description,
    decimal? Amount,
    int? Installments);
