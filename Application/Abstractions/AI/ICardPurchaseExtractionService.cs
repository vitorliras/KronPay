namespace Application.Abstractions.AI;

public interface ICardPurchaseExtractionService
{
    Task<IReadOnlyList<ExtractedCardPurchase>> ExtractFromTextAsync(
        int userId,
        string text,
        string feature,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ExtractedCardPurchase>> ExtractFromTableAsync(
        int userId,
        string tableText,
        string feature,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ExtractedCardPurchase>> ExtractFromFileAsync(
        int userId,
        AiAttachment attachment,
        string feature,
        CancellationToken cancellationToken = default);
}
