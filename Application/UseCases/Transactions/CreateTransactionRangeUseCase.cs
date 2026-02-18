using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Transactions;
using Domain.Entities.Transactions;
using Domain.Interfaces;
using Domain.Interfaces.Transactions;
using Shared.Localization;
using Shared.Results;
using System.Data.Common;
using System.Transactions;

namespace Application.UseCases.Transactions;

public sealed class CreateTransactionRangeUseCase
    : IUseCase<TransactionRangeRequest, TransactionRangeResponse>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public CreateTransactionRangeUseCase(
        ITransactionRepository transactionRepository,IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<TransactionRangeResponse>> ExecuteAsync(TransactionRangeRequest request)
    {
        var userId = _currentUser.UserId;

        if (request.Transactions is null || !request.Transactions.Any())
        {
            return ResultEntity<TransactionRangeResponse>.Failure(MessageKeys.OperationFailed);
        }

        foreach (var transaction in request.Transactions)
        {
            transaction.SetUser(userId);
        }

        var added = await _transactionRepository.AddRangeAsync(request.Transactions);

        if (!added)
            return ResultEntity<TransactionRangeResponse>.Failure("", MessageKeys.OperationFailed);

        var uow = await _unitOfWork.CommitAsync();

        if (!uow)
            return ResultEntity<TransactionRangeResponse>.Failure("", MessageKeys.OperationFailed);

        return ResultEntity<TransactionRangeResponse>.Success(
            new TransactionRangeResponse(
                 request.Transactions.Count(),
                 MessageKeys.OperationSuccess,
                 true
            ), MessageKeys.OperationSuccess
        );

    }

}
