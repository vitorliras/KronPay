using Application.Abstractions.AI;
using Application.Abstractions.Import;
using Domain.Entities.Configuration;
using Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Shared.AI;
using Shared.Localization;

namespace Infrastructure.Imports.Parsers;

public sealed class AiDocumentTransactionImportParser : ITransactionImportParser
{
    private readonly IAiDocumentReader _documentReader;
    private readonly ITransactionExtractionService _extractionService;
    private readonly ILogger<AiDocumentTransactionImportParser> _logger;

    public AiDocumentTransactionImportParser(
        IAiDocumentReader documentReader,
        ITransactionExtractionService extractionService,
        ILogger<AiDocumentTransactionImportParser> logger)
    {
        _documentReader = documentReader;
        _extractionService = extractionService;
        _logger = logger;
    }

    public bool IsAiBased => true;

    public bool CanParse(string fileName) => AiDocumentFormats.IsAiOnly(fileName);

    public async Task<IEnumerable<ImportedTransactionResponse>> ParseAsync(
        Stream fileStream,
        int userId,
        IEnumerable<PaymentMethod>? paymentMethods = null,
        IEnumerable<Category>? categories = null)
    {
        try
        {
            var document = await _documentReader.ReadAsync(fileStream);
            var extracted = await ExtractAsync(userId, document);

            if (extracted.Count == 0)
                throw new DomainException(MessageKeys.AiNothingExtracted);

            return extracted.Select(e => e.ToImportedTransaction()).ToList();
        }
        catch (DomainException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AI document extraction failed.");
            throw new DomainException(MessageKeys.AiExtractionFailed);
        }
    }

    private async Task<IReadOnlyList<ExtractedTransaction>> ExtractAsync(int userId, AiDocument document)
    {
        if (document.Attachment is not null)
        {
            return await _extractionService.ExtractFromFileAsync(
                userId,
                document.Attachment,
                AiFeatures.DocumentTransactionImport);
        }

        var result = new List<ExtractedTransaction>();

        foreach (var chunk in document.TableChunks)
        {
            result.AddRange(await _extractionService.ExtractFromTableAsync(
                userId,
                chunk,
                AiFeatures.DocumentTransactionImport));
        }

        return result;
    }
}
