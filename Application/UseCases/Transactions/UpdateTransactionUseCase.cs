using Application.Abstractions;
using Application.DTOs.Transactions;
using Domain.Interfaces;
using Domain.Interfaces.Transactions;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Transactions;

public sealed class UpdateTransactionUseCase
    : IUseCase<UpdateTransactionRequest, TransactionResponse>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUnitOfWork _uow;

    public UpdateTransactionUseCase(
        ITransactionRepository transactionRepository,
        IUnitOfWork uow)
    {
        _transactionRepository = transactionRepository;
        _uow = uow;
    }

    public async Task<ResultEntity<TransactionResponse>> ExecuteAsync(UpdateTransactionRequest request)
    {
        var transaction = await _transactionRepository
            .GetByIdAsync(request.Id, request.UserId);

        if (transaction is null)
            return ResultEntity<TransactionResponse>.Failure("", MessageKeys.TransactionNotFound);

        var affected = 1;

        if (request.UpdateGroup && transaction.TransactionGroupId.HasValue)
        {
            var result = await _transactionRepository
                .UpdatePendingByGroupAsync(
                    transaction.TransactionGroupId.Value,
                    request.UserId,
                    t =>
                    {
                        t.VerifyAmount(request.Amount);
                        t.VerifyDescription(request.Description);
                        t.VerifyCategory( request.CategoryId, request.CategoryItemId);
                    });

            if (!result)
                return ResultEntity<TransactionResponse>.Failure("", MessageKeys.OperationFailed);

            affected = (
                await _transactionRepository
                    .GetFutureByGroupAsync(
                        transaction.TransactionGroupId.Value,
                        request.UserId,
                        DateTime.UtcNow
                    )
            ).Count();
        }
        else
        {
            transaction.VerifyAmount(request.Amount);
            transaction.VerifyDescription(request.Description);
            transaction.VerifyCategory( request.CategoryId, request.CategoryItemId);

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
                affected
            ), MessageKeys.OperationSuccess
        );
    }
}
