using Application.Abstractions;
using Application.Abstractions.AI;
using Application.Abstractions.Common;
using Application.DTOs.Card.CardPurchases;
using Application.Services.Card;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Shared.AI;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Card.CardPurchases;

public sealed class ParseCardPurchasesFromTextUseCase
    : IUseCase<ParseCardPurchasesFromTextRequest, CardPurchasePreviewResponse>
{
    private readonly IAiAvailability _aiAvailability;
    private readonly ICreditCardRepository _creditCardRepository;
    private readonly ICardPurchaseExtractionService _extractionService;
    private readonly CardPurchasePreviewBuilder _previewBuilder;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<ParseCardPurchasesFromTextUseCase> _logger;

    public ParseCardPurchasesFromTextUseCase(
        IAiAvailability aiAvailability,
        ICreditCardRepository creditCardRepository,
        ICardPurchaseExtractionService extractionService,
        CardPurchasePreviewBuilder previewBuilder,
        ICurrentUserService currentUser,
        ILogger<ParseCardPurchasesFromTextUseCase> logger)
    {
        _aiAvailability = aiAvailability;
        _creditCardRepository = creditCardRepository;
        _extractionService = extractionService;
        _previewBuilder = previewBuilder;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<ResultEntity<CardPurchasePreviewResponse>> ExecuteAsync(
        ParseCardPurchasesFromTextRequest request)
    {
        if (_aiAvailability.GetActive() is null)
            return ResultEntity<CardPurchasePreviewResponse>.Failure(MessageKeys.AiUnavailable);

        var userId = _currentUser.UserId;

        var card = await _creditCardRepository.GetByIdAsync(request.CreditCardId, userId);
        if (card is null)
            return ResultEntity<CardPurchasePreviewResponse>.Failure(MessageKeys.CreditCardNotFound);

        IReadOnlyList<ExtractedCardPurchase> extracted;

        try
        {
            extracted = await _extractionService.ExtractFromTextAsync(
                userId,
                request.Text.Trim(),
                AiFeatures.CardTextEntry);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Card purchase text extraction failed.");
            return ResultEntity<CardPurchasePreviewResponse>.Failure(MessageKeys.AiExtractionFailed);
        }

        if (extracted.Count == 0)
            return ResultEntity<CardPurchasePreviewResponse>.Failure(MessageKeys.AiNothingExtracted);

        var preview = await _previewBuilder.BuildAsync(userId, card.Id, extracted, skipped: 0, useAi: true);

        return ResultEntity<CardPurchasePreviewResponse>.Success(preview, MessageKeys.OperationSuccess);
    }
}
