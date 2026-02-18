using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Transactions;
using Domain.Interfaces.Transactions;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Transactions;

public sealed class GetTransactionsByIdGroupUseCase
    : IUseCase<GetTransactionsByGroupRequest, IEnumerable<TransactionFullResponse>>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICurrentUserService _currentUser;

    public GetTransactionsByIdGroupUseCase(
        ITransactionRepository transactionRepository,
        ICurrentUserService currentUser)
    {
        _transactionRepository = transactionRepository;
        _currentUser = currentUser;

    }

    public async Task<ResultEntity<IEnumerable<TransactionFullResponse>>> ExecuteAsync(
        GetTransactionsByGroupRequest request)
    {
        var userId = _currentUser.UserId;

        var transactions = await _transactionRepository
            .GetByGroupAsync(request.TransactionGroupId, userId);

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


