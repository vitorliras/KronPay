using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Transactions;
using Domain.Interfaces;
using Domain.Interfaces.Transactions;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Transactions;

public sealed class ChangeStatusTransactionUseCase
    : IUseCase<ChangeStatusTransactionRequest, TransactionResponse>
{

    private readonly ICurrentUserService _currentUser;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUnitOfWork _uow;

    public ChangeStatusTransactionUseCase(ITransactionRepository transactionRepository, IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _transactionRepository = transactionRepository;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<TransactionResponse>> ExecuteAsync(ChangeStatusTransactionRequest request)
    {
        var userId = _currentUser.UserId;

        var transaction = await _transactionRepository
            .GetByIdAsync(request.Id, userId);

        if (transaction is null)
            return ResultEntity<TransactionResponse>.Failure("", MessageKeys.TransactionNotFound);

        else
        {
            if (request.Status.Equals("P"))  transaction.Paid();
            if (request.Status.Equals("C"))  transaction.Cancel();
            if (request.Status.Equals("O"))  transaction.Open();

            if (!await _transactionRepository.UpdateAsync(transaction))
                return ResultEntity<TransactionResponse>.Failure("", MessageKeys.OperationFailed);
        }

        if (!await _uow.CommitAsync())
            return ResultEntity<TransactionResponse>.Failure("", MessageKeys.OperationFailed);

        return ResultEntity<TransactionResponse>.Success(
            new TransactionResponse(
                transaction.Id,
                transaction.Description,
                transaction.TransactionGroupId ?? 0,
                1
            ), MessageKeys.OperationSuccess
        );
    }
}
