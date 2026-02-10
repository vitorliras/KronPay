using Application.Abstractions;
using Application.DTOs.Transactions;
using Domain.Interfaces.Transactions;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Transactions;

public sealed class GetTransactionsByYearUseCase
    : IUseCase<GetTransactionsByYearRequest, IEnumerable<TransactionFullResponse>>
{
    private readonly ITransactionRepository _transactionRepository;

    public GetTransactionsByYearUseCase(
        ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task<ResultT<IEnumerable<TransactionFullResponse>>> ExecuteAsync(
        GetTransactionsByYearRequest request)
    {
        var transactions = await _transactionRepository
            .GetByYearAsync(request.UserId, request.Year);

        var response = transactions.Select(t => new TransactionFullResponse(
            t.Id,
            t.UserId,
            t.Description,
            t.Amount,
            t.CodTypeTransaction,
            t.TransactionDate,
            t.Status,
            t.CategoryId,
            t.CategoryItemId,
            t.TransactionGroupId,
            t.CreatedAt
        ));

        return ResultT<IEnumerable<TransactionFullResponse>>.Success(
            response,
            MessageKeys.OperationSuccess
        );
    }
}


