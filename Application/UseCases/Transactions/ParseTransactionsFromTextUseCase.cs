using Application.Abstractions;
using Application.Abstractions.AI;
using Application.Abstractions.Common;
using Application.DTOs.Transactions;
using Application.Services.Transactions;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Shared.AI;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Transactions;

public sealed class ParseTransactionsFromTextUseCase
    : IUseCase<ParseTransactionsFromTextRequest, ImportTransactionsResponse>
{
    private readonly IAiAvailability _aiAvailability;
    private readonly ITransactionExtractionService _extractionService;
    private readonly ImportedTransactionClassificationService _classificationService;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<ParseTransactionsFromTextUseCase> _logger;

    public ParseTransactionsFromTextUseCase(
        IAiAvailability aiAvailability,
        ITransactionExtractionService extractionService,
        ImportedTransactionClassificationService classificationService,
        ICategoryRepository categoryRepository,
        ICurrentUserService currentUser,
        ILogger<ParseTransactionsFromTextUseCase> logger)
    {
        _aiAvailability = aiAvailability;
        _extractionService = extractionService;
        _classificationService = classificationService;
        _categoryRepository = categoryRepository;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<ResultEntity<ImportTransactionsResponse>> ExecuteAsync(
        ParseTransactionsFromTextRequest request)
    {
        if (_aiAvailability.GetActive() is null)
            return ResultEntity<ImportTransactionsResponse>.Failure(MessageKeys.AiUnavailable);

        var userId = _currentUser.UserId;

        IReadOnlyList<ExtractedTransaction> extracted;

        try
        {
            extracted = await _extractionService.ExtractFromTextAsync(
                userId,
                request.Text.Trim(),
                AiFeatures.TextTransactionEntry);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Free text transaction extraction failed.");
            return ResultEntity<ImportTransactionsResponse>.Failure(MessageKeys.AiExtractionFailed);
        }

        if (extracted.Count == 0)
            return ResultEntity<ImportTransactionsResponse>.Failure(MessageKeys.AiNothingExtracted);

        var imported = extracted.Select(e => e.ToImportedTransaction()).ToList();
        var categories = (await _categoryRepository.GetAllAsync(userId)).ToList();

        var classified = await _classificationService.ClassifyAsync(userId, imported, categories, useAi: true);

        var transactions = classified
            .OfType<ImportedTransactionResponse>()
            .OrderBy(t => t.TransactionDate)
            .ToList();

        return ResultEntity<ImportTransactionsResponse>.Success(
            new ImportTransactionsResponse(
                TotalRead: imported.Count,
                TotalImported: 0,
                TotalSkipped: imported.Count - transactions.Count,
                Transactions: transactions),
            MessageKeys.OperationSuccess);
    }
}
