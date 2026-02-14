using Application.Abstractions;
using Application.DTOs.Transactions;
using Domain.Interfaces.Transactions;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Transactions;

public sealed class GetTransactionsByIdGroupUseCase
    : IUseCase<GetTransactionsByGroupRequest, IEnumerable<TransactionFullResponse>>
{
    private readonly ITransactionRepository _transactionRepository;

    public GetTransactionsByIdGroupUseCase(
        ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task<ResultEntity<IEnumerable<TransactionFullResponse>>> ExecuteAsync(
        GetTransactionsByGroupRequest request)
    {
        var transactions = await _transactionRepository
            .GetByGroupAsync(request.TransactionGroupId, request.UserId);

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


        return ResultEntity<IEnumerable<TransactionFullResponse>>.Success(
            response,
            MessageKeys.OperationSuccess
        );
    }
}


