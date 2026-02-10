using Application.Abstractions;
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

    public CreateTransactionUseCase(
        ITransactionRepository transactionRepository,
        ITransactionGroupRepository groupRepository,
        IUnitOfWork uow)
    {
        _transactionRepository = transactionRepository;
        _groupRepository = groupRepository;
        _uow = uow;
    }

    public async Task<ResultT<TransactionResponse>> ExecuteAsync(
        CreateTransactionRequest request)
    {
        TransactionGroup? group = null;

        if (request.RecurrenceType == "F")
        {
            group = TransactionGroup.CreateFixed(
                request.UserId,
                request.TransactionDate,
                request.Installments
            );

            var groupResult = await _groupRepository.AddAsync(group);
            if (!groupResult)
                return ResultT<TransactionResponse>.Failure("", MessageKeys.OperationFailed);
        }

        if (request.RecurrenceType == "I")
        {
            group = TransactionGroup.CreateInfinite(
                request.UserId,
                request.TransactionDate
            );

            var groupResult = await _groupRepository.AddAsync(group);
            if (!groupResult)
                return ResultT<TransactionResponse>.Failure("", MessageKeys.OperationFailed);
        }

        var transactions = new List<Transaction>();

        if (group is null || request.Installments <= 1)
        {
            transactions.Add(new Transaction(
                request.UserId,
                request.Amount,
                request.TransactionDate,
                request.Description,
                request.CodTypeTransaction,
                request.CategoryId,
                request.CategoryItemId,
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

                transactions.Add(new Transaction(
                    request.UserId,
                    request.Amount,
                    date,
                    request.Description,
                    request.CodTypeTransaction,
                    request.CategoryId,
                    request.CategoryItemId,
                    group
                ));
            }
        }

        var addResult = transactions.Count == 1
            ? await _transactionRepository.AddAsync(transactions[0])
            : await _transactionRepository.AddRangeAsync(transactions);

        if (!addResult)
            return ResultT<TransactionResponse>.Failure("", MessageKeys.OperationFailed);

        var committed = await _uow.CommitAsync();
        if (!committed)
            return ResultT<TransactionResponse>.Failure("", MessageKeys.OperationFailed);

        var mainTransaction = transactions.First();

        return ResultT<TransactionResponse>.Success(
            new TransactionResponse(
                mainTransaction.Id,
                mainTransaction.Description,
                mainTransaction.TransactionGroupId ?? 0,
                transactions.Count,
                MessageKeys.OperationSuccess,
                true
            )

        );
    }
}
