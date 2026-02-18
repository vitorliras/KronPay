using Application.Abstractions;
using Application.Abstractions.Common;
using Application.Abstractions.Import;
using Application.DTOs.Transactions;
using Domain.Entities.Transactions;
using Domain.Interfaces;
using Domain.Interfaces.Transactions;
using Shared.Localization;
using Shared.Results;
using System.Transactions;

public sealed class ImportTransactionsUseCase
    : IUseCase<ImportTransactionsRequest, ImportTransactionsResponse>
{
    private readonly IEnumerable<ITransactionImportParser> _parsers;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ITransactionAiClassifier _aiClassifier;
    private readonly ITransactionAiBatchClassifier _batchAiClassifier;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public ImportTransactionsUseCase(
        IEnumerable<ITransactionImportParser> parsers,
        ITransactionRepository transactionRepository,
        ITransactionAiClassifier aiClassifier,
        ITransactionAiBatchClassifier batchAiClassifier,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _parsers = parsers;
        _transactionRepository = transactionRepository;
        _aiClassifier = aiClassifier;
        _unitOfWork = unitOfWork;
        _batchAiClassifier = batchAiClassifier;
        _currentUser = currentUser;
    }

    //public async Task<ResultEntity<ImportTransactionsResponse>> ExecuteAsync(ImportTransactionsRequest request)
    //{
    //    var parser = _parsers.FirstOrDefault(p => p.CanParse(request.FileName));
    //    var transactions = new List<Domain.Entities.Transactions.Transaction>();

    //    if (parser is null)
    //        return ResultEntity<ImportTransactionsResponse>.Failure("", MessageKeys.OperationFailed);
    //    //throw new InvalidOperationException("Formato de arquivo não suportado.");

    //    var importedTransactions = (await parser.ParseAsync(
    //        request.FileStream,
    //        userId)).ToList();

    //    var totalRead = importedTransactions.Count;
    //    var totalImported = 0;
    //    var totalSkipped = 0;

    //    var updatedImportedTransactions = new List<ImportedTransactionResponse>();

    //    foreach (var imported in importedTransactions)
    //    {
    //        try
    //        {
    //            var updated = imported;

    //            var suggestion = await _aiClassifier.SuggestAsync(
    //                userId,
    //                imported.Description,
    //                imported.Amount,
    //                request.UseAi
    //            );

    //            if (suggestion.Confidence >= 0.70m)
    //            {
    //                if (suggestion.IsInvestment)
    //                {
    //                    updated = updated with
    //                    {
    //                        Type = "V",
    //                        CategoryId = suggestion.SuggestedCategoryId ?? updated.CategoryId
    //                    };
    //                }
    //                else
    //                {
    //                    updated = updated with
    //                    {
    //                        CategoryId = suggestion.SuggestedCategoryId ?? updated.CategoryId
    //                    };
    //                }
    //            }

    //            if (request.Preview)
    //                updatedImportedTransactions.Add(updated);

    //            if (!request.Preview)
    //            {
    //                transactions.Add(new Domain.Entities.Transactions.Transaction(
    //                    userId: userId,
    //                    amount: updated.Amount,
    //                    transactionDate: updated.TransactionDate,
    //                    description: updated.Description,
    //                    codTypeTransaction: updated.Type,
    //                    categoryId: updated.CategoryId,
    //                    categoryItemId: updated.CategoryItemId
    //                ));
    //            }

    //            totalImported++;
    //        }
    //        catch
    //        {
    //            totalSkipped++;
    //        }
    //    }

    //    if (request.Preview)
    //    {
    //        return ResultEntity<ImportTransactionsResponse>.Success(
    //           new ImportTransactionsResponse(
    //                TotalRead: totalRead,
    //                TotalImported: 0,
    //                TotalSkipped: totalRead,
    //                Transactions: importedTransactions
    //           )
    //        );
    //    }

    //    var add = await _transactionRepository.AddRangeAsync(transactions);

    //    if (!add)
    //        return ResultEntity<ImportTransactionsResponse>.Failure("", MessageKeys.OperationFailed);

    //    var commited = await _unitOfWork.CommitAsync();

    //    if (!commited)
    //        return ResultEntity<ImportTransactionsResponse>.Failure("", MessageKeys.OperationFailed);

    //    return ResultEntity<ImportTransactionsResponse>.Success(
    //        new ImportTransactionsResponse(
    //            TotalRead: totalRead,
    //            TotalImported: totalImported,
    //            TotalSkipped: totalSkipped,
    //            Transactions: importedTransactions
    //        )
    //    );

    //}

    public async Task<ResultEntity<ImportTransactionsResponse>> ExecuteAsync(
    ImportTransactionsRequest request)
    {
        var userId = _currentUser.UserId;

        var parser = _parsers.FirstOrDefault(p => p.CanParse(request.FileName));
        if (parser is null)
            return ResultEntity<ImportTransactionsResponse>.Failure("", MessageKeys.OperationFailed);

        var importedTransactions = (await parser.ParseAsync(
            request.FileStream,
            userId)).ToList();

        var totalRead = importedTransactions.Count;
        var totalImported = 0;
        var totalSkipped = 0;

        var updatedImportedTransactions = new List<ImportedTransactionResponse>();
        var transactionsToSave = new List<Domain.Entities.Transactions.Transaction>();

        var batches = importedTransactions
            .Select((t, i) => new { t, i })
            .GroupBy(x => x.i / 15)
            .Select(g => g.Select(x => x.t).ToList())
            .ToList();

        foreach (var batch in batches)
        {
            IReadOnlyList<TransactionAiSuggestion> suggestions;

            try
            {
                suggestions = await _batchAiClassifier.SuggestBatchAsync(
                    userId,
                    batch,
                    request.UseAi
                );
            }
            catch
            {
                totalSkipped += batch.Count;
                continue;
            }

            for (int i = 0; i < batch.Count; i++)
            {
                try
                {
                    var original = batch[i];
                    var suggestion = suggestions[i];

                    var updated = original;

                    if (suggestion.Confidence >= 0.70m)
                    {
                        if (suggestion.IsInvestment)
                        {
                            updated = updated with
                            {
                                Type = "V",
                                CategoryId = suggestion.SuggestedCategoryId ?? updated.CategoryId
                            };
                        }
                        else if (suggestion.SuggestedCategoryId is not null)
                        {
                            updated = updated with
                            {
                                CategoryId = suggestion.SuggestedCategoryId.Value
                            };
                        }
                    }

                    updatedImportedTransactions.Add(updated);

                    if (!request.Preview)
                    {
                        transactionsToSave.Add(new Domain.Entities.Transactions.Transaction(
                            userId: userId,
                            amount: updated.Amount,
                            transactionDate: updated.TransactionDate,
                            description: updated.Description,
                            codTypeTransaction: updated.Type,
                            categoryId: updated.CategoryId,
                            categoryItemId: updated.CategoryItemId
                        ));
                    }

                    totalImported++;
                }
                catch
                {
                    totalSkipped++;
                }
            }
        }

        if (request.Preview)
        {
            return ResultEntity<ImportTransactionsResponse>.Success(
                new ImportTransactionsResponse(
                    TotalRead: totalRead,
                    TotalImported: 0,
                    TotalSkipped: totalRead,
                    Transactions: updatedImportedTransactions
                )
            );
        }

        var add = await _transactionRepository.AddRangeAsync(transactionsToSave);
        if (!add)
            return ResultEntity<ImportTransactionsResponse>.Failure("", MessageKeys.OperationFailed);

        var committed = await _unitOfWork.CommitAsync();
        if (!committed)
            return ResultEntity<ImportTransactionsResponse>.Failure("", MessageKeys.OperationFailed);

        return ResultEntity<ImportTransactionsResponse>.Success(
            new ImportTransactionsResponse(
                TotalRead: totalRead,
                TotalImported: totalImported,
                TotalSkipped: totalSkipped,
                Transactions: updatedImportedTransactions
            ), MessageKeys.OperationSuccess
        );
    }

}
