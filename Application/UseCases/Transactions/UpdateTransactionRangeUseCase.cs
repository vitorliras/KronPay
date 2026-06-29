using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Transactions;
using Domain.Interfaces;
using Domain.Interfaces.Transactions;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Transactions;

public sealed class UpdateTransactionRangeUseCase
    : IUseCase<UpdtadeRangeTransaction, TransactionRangeResponse>
{

    private readonly ICurrentUserService _currentUser;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUnitOfWork _uow;

    public UpdateTransactionRangeUseCase(ITransactionRepository transactionRepository, IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _transactionRepository = transactionRepository;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<TransactionRangeResponse>> ExecuteAsync(UpdtadeRangeTransaction requests)
    {
        var userId = _currentUser.UserId;

        foreach (var request in requests.Transactions)
        {
            
            var transaction = await _transactionRepository
                .GetByIdAsync(request.Id, userId);

            if (transaction is null)
                return ResultEntity<TransactionRangeResponse>.Failure(MessageKeys.TransactionNotFound);

            else
            {

                if (request.Status.Equals("P")) transaction.Paid();
                if (request.Status.Equals("C")) transaction.Cancel();
                if (request.Status.Equals("O")) transaction.Open();

                transaction.UpdateAmount(request.Amount);
                transaction.UpdateDescription(request.Description);
                transaction.SetDate(request.TransactionDate);
                transaction.UpdateCategory(request.CategoryId, request.CategoryItemId);

                if (!await _transactionRepository.UpdateAsync(transaction))
                    return ResultEntity<TransactionRangeResponse>.Failure(MessageKeys.UpdateFailed);
            }

        }


        if (!await _uow.CommitAsync())
            return ResultEntity<TransactionRangeResponse>.Failure(MessageKeys.DataPersistenceFailed);

        return ResultEntity<TransactionRangeResponse>.Success(
            new TransactionRangeResponse(
                requests.Transactions.Count()
            ), MessageKeys.OperationSuccess
        );
    }
}
