using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Transactions;
using Domain.Interfaces;
using Domain.Interfaces.Transactions;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Transactions;

public sealed class DeleteTransactionUseCase
    : IUseCase<DeleteTransactionRequest, TransactionResponse>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ITransactionGroupRepository _groupRepository;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DeleteTransactionUseCase(
        ITransactionRepository transactionRepository,
        ITransactionGroupRepository groupRepository,
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _transactionRepository = transactionRepository;
        _groupRepository = groupRepository;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<TransactionResponse>> ExecuteAsync(DeleteTransactionRequest request)
    {
        var userId = _currentUser.UserId;

        var transaction = await _transactionRepository.GetByIdAsync(request.TransactionId, userId);

        if (transaction is null)
            return ResultEntity<TransactionResponse>.Failure("", MessageKeys.TransactionNotFound);

        int affected = 0;

        if (!request.DeleteGroup || transaction.TransactionGroupId is null)
        {
            var deleted = await _transactionRepository.DeleteAsync(transaction);
            if (!deleted)
                return ResultEntity<TransactionResponse>.Failure("", MessageKeys.OperationFailed);

            affected = 1;

            if (transaction.TransactionGroupId is not null)
            {
                var transactions = _transactionRepository.
                       GetByGroupAsync(transaction.TransactionGroupId.Value, userId).Result.Count();

                if (transactions <= 1)
                {
                    var group = await _groupRepository.GetByIdAsync(transaction.TransactionGroupId.Value, userId);

                    if(group is not null)
                    {
                        deleted = await _groupRepository.DeleteAsync(group);
                        if (!deleted)
                            return ResultEntity<TransactionResponse>.Failure("", MessageKeys.OperationFailed);
                    }
                }
            }

        }
        else
        {
            var groupId = transaction.TransactionGroupId.Value;

            if (request.FromDate.HasValue)
            {
                affected = _transactionRepository.
                    GetFutureByGroupAsync(groupId, userId, request.FromDate.Value).Result.Count();

                var deleted = await _transactionRepository
                    .DeleteFutureByGroupAsync(
                        groupId,
                        userId,
                        request.FromDate.Value
                    );

                if (!deleted)
                    return ResultEntity<TransactionResponse>.Failure("", MessageKeys.OperationFailed);


            }
            else
            {
                affected = _transactionRepository.
                   GetByGroupAsync(groupId, userId).Result.Count();

                var deleted = await _transactionRepository.DeleteByGroupAsync(groupId, userId);

                if (!deleted)
                    return ResultEntity<TransactionResponse>.Failure("", MessageKeys.OperationFailed);

                var group = await _groupRepository.GetByIdAsync(transaction.TransactionGroupId.Value, userId);

                if (group is not null)
                {
                    deleted = await _groupRepository.DeleteAsync(group);
                    if (!deleted)
                        return ResultEntity<TransactionResponse>.Failure("", MessageKeys.OperationFailed);
                }

            }
        }

        var committed = await _uow.CommitAsync();
        if (!committed)
            return ResultEntity<TransactionResponse>
                .Failure("", MessageKeys.OperationFailed);

        return ResultEntity<TransactionResponse>.Success(
            new TransactionResponse(
                transaction.Id,
                transaction.Description,
                transaction.TransactionGroupId ?? 0,
                affected
            ), MessageKeys.OperationSuccess
        );
    }
}
