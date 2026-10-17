using Application.Abstractions;
using Application.Abstractions.AI;
using Application.Abstractions.Common;
using Application.Abstractions.Import;
using Application.DTOs.Card.CardPurchases;
using Application.Services.Card;
using Domain.Entities.Configuration;
using Domain.Exceptions;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Shared.AI;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Card.CardPurchases;

public sealed class ImportCardPurchasesUseCase
    : IUseCase<ImportCardPurchasesRequest, CardPurchasePreviewResponse>
{
    private readonly IEnumerable<ITransactionImportParser> _parsers;
    private readonly IAiAvailability _aiAvailability;
    private readonly IAiDocumentReader _documentReader;
    private readonly ICardPurchaseExtractionService _extractionService;
    private readonly ICreditCardRepository _creditCardRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly CardPurchasePreviewBuilder _previewBuilder;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<ImportCardPurchasesUseCase> _logger;

    public ImportCardPurchasesUseCase(
        IEnumerable<ITransactionImportParser> parsers,
        IAiAvailability aiAvailability,
        IAiDocumentReader documentReader,
        ICardPurchaseExtractionService extractionService,
        ICreditCardRepository creditCardRepository,
        ICategoryRepository categoryRepository,
        CardPurchasePreviewBuilder previewBuilder,
        ICurrentUserService currentUser,
        ILogger<ImportCardPurchasesUseCase> logger)
    {
        _parsers = parsers;
        _aiAvailability = aiAvailability;
        _documentReader = documentReader;
        _extractionService = extractionService;
        _creditCardRepository = creditCardRepository;
        _categoryRepository = categoryRepository;
        _previewBuilder = previewBuilder;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<ResultEntity<CardPurchasePreviewResponse>> ExecuteAsync(ImportCardPurchasesRequest request)
    {
        var userId = _currentUser.UserId;

        var card = await _creditCardRepository.GetByIdAsync(request.CreditCardId, userId);
        if (card is null)
            return ResultEntity<CardPurchasePreviewResponse>.Failure(MessageKeys.CreditCardNotFound);

        var aiOnly = AiDocumentFormats.IsAiOnly(request.FileName);
        var localParser = aiOnly ? null : _parsers.FirstOrDefault(p => !p.IsAiBased && p.CanParse(request.FileName));

        if (!aiOnly && localParser is null)
            return ResultEntity<CardPurchasePreviewResponse>.Failure(MessageKeys.UnsupportedFileFormat);

        var bytes = await ReadAllBytesAsync(request.FileStream);

        try
        {
            var read = aiOnly
                ? await ReadWithAiAsync(userId, bytes)
                : await ReadLocallyAsync(userId, localParser!, request.FileName, bytes);

            if (read.Purchases.Count == 0)
                return ResultEntity<CardPurchasePreviewResponse>.Failure(MessageKeys.AiNothingExtracted);

            var preview = await _previewBuilder.BuildAsync(
                userId,
                card.Id,
                read.Purchases,
                read.Skipped,
                useAi: request.UseAi || read.AiBased);

            return ResultEntity<CardPurchasePreviewResponse>.Success(preview, MessageKeys.OperationSuccess);
        }
        catch (DomainException ex)
        {
            return ResultEntity<CardPurchasePreviewResponse>.Failure(ex.Message);
        }
    }

    private async Task<ReadResult> ReadLocallyAsync(
        int userId,
        ITransactionImportParser parser,
        string fileName,
        byte[] bytes)
    {
        var canFallBackToAi = fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase)
            && _aiAvailability.GetActive() is not null;

        List<ImportedTransactionResponse>? parsed = null;

        try
        {
            var categories = (await _categoryRepository.GetAllAsync(userId)).ToList();

            parsed = (await parser.ParseAsync(
                new MemoryStream(bytes),
                userId,
                Array.Empty<PaymentMethod>(),
                categories)).ToList();
        }
        catch (DomainException)
        {
            throw;
        }
        catch when (!canFallBackToAi)
        {
            throw;
        }
        catch
        {
            parsed = null;
        }

        if (parsed is null || parsed.Count == 0)
            return canFallBackToAi ? await ReadWithAiAsync(userId, bytes) : new ReadResult(new List<ExtractedCardPurchase>(), 0, false);

        var purchases = parsed
            .Where(p => p.Type == "E")
            .Select(p => new ExtractedCardPurchase(
                p.TransactionDate,
                Truncate(p.Description),
                p.Amount,
                1))
            .ToList();

        return new ReadResult(purchases, parsed.Count - purchases.Count, false);
    }

    private async Task<ReadResult> ReadWithAiAsync(int userId, byte[] bytes)
    {
        try
        {
            var document = await _documentReader.ReadAsync(new MemoryStream(bytes));

            var purchases = new List<ExtractedCardPurchase>();

            if (document.Attachment is not null)
            {
                purchases.AddRange(await _extractionService.ExtractFromFileAsync(
                    userId,
                    document.Attachment,
                    AiFeatures.CardDocumentImport));
            }
            else
            {
                foreach (var chunk in document.TableChunks)
                {
                    purchases.AddRange(await _extractionService.ExtractFromTableAsync(
                        userId,
                        chunk,
                        AiFeatures.CardDocumentImport));
                }
            }

            return new ReadResult(purchases, 0, true);
        }
        catch (DomainException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Card statement AI extraction failed.");
            throw new DomainException(MessageKeys.AiExtractionFailed);
        }
    }

    private static string Truncate(string description) =>
        description.Length <= ExtractedCardPurchaseValidator.MaxDescriptionLength
            ? description
            : description[..ExtractedCardPurchaseValidator.MaxDescriptionLength].TrimEnd();

    private static async Task<byte[]> ReadAllBytesAsync(Stream stream)
    {
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory);
        return memory.ToArray();
    }

    private sealed record ReadResult(List<ExtractedCardPurchase> Purchases, int Skipped, bool AiBased);
}
