using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Configuration.Categories;
using Application.DTOs.Transactions;
using Domain.Entities.Transactions;
using Domain.Interfaces;
using Domain.Interfaces.Transactions;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Transactions;

public sealed class CreateTransactionUseCase
    : IUseCase<CreateTransactionRequest, TransactionResponse>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ITransactionGroupRepository _groupRepository;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreateTransactionUseCase(
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

    public async Task<ResultEntity<TransactionResponse>> ExecuteAsync(CreateTransactionRequest request)
    {
        var userId = _currentUser.UserId;

        TransactionGroup? group = null;

        if (request.RecurrenceType == "F")
        {
            group = TransactionGroup.CreateFixed(
                userId,
                request.TransactionDate,
                request.Installments
            );

            var groupResult = await _groupRepository.AddAsync(group);
            if (!groupResult)
                return ResultEntity<TransactionResponse>.Failure("", MessageKeys.OperationFailed);
        }

        if (request.RecurrenceType == "I")
        {
            group = TransactionGroup.CreateInfinite(
                userId,
                request.TransactionDate
            );

            var groupResult = await _groupRepository.AddAsync(group);
            if (!groupResult)
                return ResultEntity<TransactionResponse>.Failure("", MessageKeys.OperationFailed);
        }

        var transactions = new List<Transaction>();

        if (group is null || request.Installments <= 1)
        {
            transactions.Add(new Transaction(
                userId,
                request.Amount,
                request.TransactionDate,
                request.Description,
                request.CodTypeTransaction,
                request.CategoryId,
                request.CategoryItemId,
                null,
                request.idMethodPayment,
                group
            ));
        }
        else
        {
            var total = group.Installments;

            for (var i = 0; i < total; i++)
            {
                var date = request.TransactionDate.AddMonths(i);

                if (group.EndDate.HasValue && date > group.EndDate.Value)
                    break;

                short installment = (short)(i + 1);

                transactions.Add(new Transaction(
                    userId,
                    request.Amount,
                    date,
                    request.Description,
                    request.CodTypeTransaction,
                    request.CategoryId,
                    request.CategoryItemId,
                    installment,
                    request.idMethodPayment,
                    group
                ));
            }
        }

        var addResult = transactions.Count == 1
            ? await _transactionRepository.AddAsync(transactions[0])
            : await _transactionRepository.AddRangeAsync(transactions);

        if (!addResult)
            return ResultEntity<TransactionResponse>.Failure("", MessageKeys.OperationFailed);

        var committed = await _uow.CommitAsync();
        if (!committed)
            return ResultEntity<TransactionResponse>.Failure("", MessageKeys.OperationFailed);

        var mainTransaction = transactions.First();

        return ResultEntity<TransactionResponse>.Success(
            new TransactionResponse(
                mainTransaction.Id,
                mainTransaction.Description,
                mainTransaction.TransactionGroupId ?? 0,
                transactions.Count
            ), MessageKeys.OperationSuccess

        );
    }
}
