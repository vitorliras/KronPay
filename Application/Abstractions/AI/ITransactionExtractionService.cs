namespace Application.Abstractions.AI;

public interface ITransactionExtractionService
{
    Task<IReadOnlyList<ExtractedTransaction>> ExtractFromTextAsync(
        int userId,
        string text,
        string feature,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ExtractedTransaction>> ExtractFromTableAsync(
        int userId,
        string tableText,
        string feature,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ExtractedTransaction>> ExtractFromFileAsync(
        int userId,
        AiAttachment attachment,
        string feature,
        CancellationToken cancellationToken = default);
}
