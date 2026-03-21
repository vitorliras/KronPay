using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Transactions;
using Domain.Entities.Transactions;
using Domain.Interfaces;
using Domain.Interfaces.Transactions;
using KronPay.Domain.Entities.Users;
using Shared.Localization;
using Shared.Results;
using System.Data.Common;
using System.Transactions;

namespace Application.UseCases.Transactions;

public sealed class DeleteTransactionRangeUseCase
    : IUseCase<DeactivateTransactionSelectRequest, TransactionRangeResponse>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DeleteTransactionRangeUseCase(
        ITransactionRepository transactionRepository,
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _transactionRepository = transactionRepository;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<TransactionRangeResponse>> ExecuteAsync(DeactivateTransactionSelectRequest request)
    {
        var userId = _currentUser.UserId;

        foreach (var item in request.Transactions)
        {

            var transactions = await _transactionRepository.GetAllTransactionAsync(item.Id);

            if (transactions is not null && transactions.Count() > 0)
                return ResultEntity<TransactionRangeResponse>.Failure(MessageKeys.ExistsAnotherRegister);

            var transaction = await _transactionRepository.GetByIdAsync(item.Id, userId);
            if (transaction is null)
                return ResultEntity<TransactionRangeResponse>.Failure(MessageKeys.CategoryNotFound);

            var result = await _transactionRepository.DeleteAsync(transaction);

            if (!result)
                return ResultEntity<TransactionRangeResponse>.Failure(MessageKeys.OperationFailed);
        }

        var uow = await _uow.CommitAsync();

        if (!uow)
            return ResultEntity<TransactionRangeResponse>.Failure(MessageKeys.OperationFailed);

        return ResultEntity<TransactionRangeResponse>.Success(
            new TransactionRangeResponse(
                 request.Transactions.Count()
            ), MessageKeys.OperationSuccess
        );

    }

}
