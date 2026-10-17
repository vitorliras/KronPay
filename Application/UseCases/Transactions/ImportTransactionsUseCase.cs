using Application.Abstractions;
using Application.Abstractions.AI;
using Application.Abstractions.Common;
using Application.Abstractions.Import;
using Application.Services.Transactions;
using Domain.Entities.Configuration;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Interfaces.Transactions;
using Shared.Localization;
using Shared.Results;

public sealed class ImportTransactionsUseCase
    : IUseCase<ImportTransactionsRequest, ImportTransactionsResponse>
{
    private readonly IEnumerable<ITransactionImportParser> _parsers;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IPaymentMethodRepository _paymentMethodRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ImportedTransactionClassificationService _classificationService;
    private readonly IAiAvailability _aiAvailability;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public ImportTransactionsUseCase(
        IEnumerable<ITransactionImportParser> parsers,
        ITransactionRepository transactionRepository,
        ImportedTransactionClassificationService classificationService,
        IAiAvailability aiAvailability,
        IPaymentMethodRepository paymentMethodRepository,
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _parsers = parsers;
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
        _paymentMethodRepository = paymentMethodRepository;
        _categoryRepository = categoryRepository;
        _classificationService = classificationService;
        _aiAvailability = aiAvailability;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<ImportTransactionsResponse>> ExecuteAsync(
        ImportTransactionsRequest request)
    {
        var userId = _currentUser.UserId;

        var parser = _parsers.FirstOrDefault(p => p.CanParse(request.FileName));
        if (parser is null)
            return ResultEntity<ImportTransactionsResponse>.Failure(MessageKeys.UnsupportedFileFormat);

        var paymentMethods = await _paymentMethodRepository.GetAllAsync(userId);
        var categories = (await _categoryRepository.GetAllAsync(userId)).ToList();

        var fileBytes = await ReadAllBytesAsync(request.FileStream);

        ParsedFile parsedFile;

        try
        {
            parsedFile = await ParseAsync(parser, request.FileName, fileBytes, userId, paymentMethods, categories);
        }
        catch (DomainException ex)
        {
            return ResultEntity<ImportTransactionsResponse>.Failure(ex.Message);
        }

        var importedTransactions = parsedFile.Transactions;
        var preview = request.Preview || parsedFile.AiBased;
        var useAi = request.UseAi || parsedFile.AiBased;

        var totalRead = importedTransactions.Count;

        var classified = await _classificationService.ClassifyAsync(
            userId,
            importedTransactions,
            categories,
            useAi);

        var updatedImportedTransactions = new List<ImportedTransactionResponse>();
        var transactionsToSave = new List<Domain.Entities.Transactions.Transaction>();

        foreach (var updated in classified.OfType<ImportedTransactionResponse>())
        {
            if (!preview)
            {
                try
                {
                    transactionsToSave.Add(new Domain.Entities.Transactions.Transaction(
                        userId: userId,
                        amount: updated.Amount,
                        transactionDate: updated.TransactionDate,
                        description: updated.Description,
                        codTypeTransaction: updated.Type,
                        categoryId: updated.CategoryId,
                        categoryItemId: updated.CategoryItemId,
                        idPaymentMethod: 1
                    ));
                }
                catch
                {
                    continue;
                }
            }

            updatedImportedTransactions.Add(updated);
        }

        var totalImported = updatedImportedTransactions.Count;
        var totalSkipped = totalRead - totalImported;

        if (preview)
        {
            return ResultEntity<ImportTransactionsResponse>.Success(
                new ImportTransactionsResponse(
                    TotalRead: totalRead,
                    TotalImported: 0,
                    TotalSkipped: totalRead,
                    Transactions: updatedImportedTransactions.OrderBy(x => x.TransactionDate)
                )
            );
        }

        var add = await _transactionRepository.AddRangeAsync(transactionsToSave);
        if (!add)
            return ResultEntity<ImportTransactionsResponse>.Failure(MessageKeys.InsertFalied);

        var committed = await _unitOfWork.CommitAsync();

        if (!committed)
            return ResultEntity<ImportTransactionsResponse>.Failure(MessageKeys.DataPersistenceFailed);

        var responseTransactions = updatedImportedTransactions
            .Select((t, index) => t with
            {
                Id = transactionsToSave[index].Id
            })
            .ToList();

        return ResultEntity<ImportTransactionsResponse>.Success(
            new ImportTransactionsResponse(
                TotalRead: totalRead,
                TotalImported: totalImported,
                TotalSkipped: totalSkipped,
                Transactions: responseTransactions
            ), MessageKeys.OperationSuccess
        );
    }

    private async Task<ParsedFile> ParseAsync(
        ITransactionImportParser parser,
        string fileName,
        byte[] fileBytes,
        int userId,
        IEnumerable<PaymentMethod> paymentMethods,
        IEnumerable<Category> categories)
    {
        var fallbackParser = IsCsv(fileName) && !parser.IsAiBased && _aiAvailability.GetActive() is not null
            ? _parsers.FirstOrDefault(p => p.IsAiBased)
            : null;

        List<ImportedTransactionResponse>? parsed = null;

        try
        {
            parsed = (await parser.ParseAsync(new MemoryStream(fileBytes), userId, paymentMethods, categories)).ToList();
        }
        catch (DomainException)
        {
            throw;
        }
        catch when (fallbackParser is null)
        {
            throw;
        }
        catch
        {
            parsed = null;
        }

        if ((parsed is null || parsed.Count == 0) && fallbackParser is not null)
        {
            var aiParsed = await fallbackParser.ParseAsync(new MemoryStream(fileBytes), userId, paymentMethods, categories);

            return new ParsedFile(aiParsed.ToList(), AiBased: true);
        }

        return new ParsedFile(parsed ?? new List<ImportedTransactionResponse>(), parser.IsAiBased);
    }

    private static bool IsCsv(string fileName) =>
        fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase);

    private static async Task<byte[]> ReadAllBytesAsync(Stream stream)
    {
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory);
        return memory.ToArray();
    }

    private sealed record ParsedFile(List<ImportedTransactionResponse> Transactions, bool AiBased);
}
