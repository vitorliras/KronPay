using Application.Abstractions;
using Application.DTOs.Transactions;
using Domain.Entities.Transactions;
using Domain.Interfaces;
using Domain.Interfaces.Transactions;
using Shared.Localization;
using Shared.Results;
using System.Data.SqlTypes;
using System.Text.RegularExpressions;
using System.Transactions;

namespace Application.UseCases.Transactions;

public sealed class DeleteTransactionUseCase
    : IUseCase<DeleteTransactionRequest, TransactionResponse>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ITransactionGroupRepository _groupRepository;
    private readonly IUnitOfWork _uow;

    public DeleteTransactionUseCase(
        ITransactionRepository transactionRepository,
        ITransactionGroupRepository groupRepository,
        IUnitOfWork uow)
    {
        _transactionRepository = transactionRepository;
        _groupRepository = groupRepository;
        _uow = uow;
    }

    public async Task<ResultT<TransactionResponse>> ExecuteAsync(DeleteTransactionRequest request)
    {
        var transaction = await _transactionRepository.GetByIdAsync(request.TransactionId, request.UserId);

        if (transaction is null)
            return ResultT<TransactionResponse>.Failure("", MessageKeys.TransactionNotFound);

        int affected = 0;

        if (!request.DeleteGroup || transaction.TransactionGroupId is null)
        {
            var deleted = await _transactionRepository.DeleteAsync(transaction);
            if (!deleted)
                return ResultT<TransactionResponse>.Failure("", MessageKeys.OperationFailed);

            affected = 1;

            if (transaction.TransactionGroupId is not null)
            {
                var transactions = _transactionRepository.
                       GetByGroupAsync(transaction.TransactionGroupId.Value, request.UserId).Result.Count();

                if (transactions <= 1)
                {
                    var group = await _groupRepository.GetByIdAsync(transaction.TransactionGroupId.Value, request.UserId);

                    if(group is not null)
                    {
                        deleted = await _groupRepository.DeleteAsync(group);
                        if (!deleted)
                            return ResultT<TransactionResponse>.Failure("", MessageKeys.OperationFailed);
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
                    GetFutureByGroupAsync(groupId, request.UserId, request.FromDate.Value).Result.Count();

                var deleted = await _transactionRepository
                    .DeleteFutureByGroupAsync(
                        groupId,
                        request.UserId,
                        request.FromDate.Value
                    );

                if (!deleted)
                    return ResultT<TransactionResponse>.Failure("", MessageKeys.OperationFailed);


            }
            else
            {
                affected = _transactionRepository.
                   GetByGroupAsync(groupId, request.UserId).Result.Count();

                var deleted = await _transactionRepository.DeleteByGroupAsync(groupId, request.UserId);

                if (!deleted)
                    return ResultT<TransactionResponse>.Failure("", MessageKeys.OperationFailed);

                var group = await _groupRepository.GetByIdAsync(transaction.TransactionGroupId.Value, request.UserId);

                if (group is not null)
                {
                    deleted = await _groupRepository.DeleteAsync(group);
                    if (!deleted)
                        return ResultT<TransactionResponse>.Failure("", MessageKeys.OperationFailed);
                }

            }
        }

        var committed = await _uow.CommitAsync();
        if (!committed)
            return ResultT<TransactionResponse>
                .Failure("", MessageKeys.OperationFailed);

        return ResultT<TransactionResponse>.Success(
            new TransactionResponse(
                transaction.Id,
                transaction.Description,
                transaction.TransactionGroupId ?? 0,
                affected,
                MessageKeys.OperationSuccess,
                true
            )
        );
    }
}
