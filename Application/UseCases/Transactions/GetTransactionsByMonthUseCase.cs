using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Transactions;
using Domain.Interfaces.Transactions;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Transactions;

public sealed class GetTransactionsByMonthUseCase
    : IUseCase<GetTransactionsByMonthRequest, IEnumerable<TransactionFullResponse>>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICurrentUserService _currentUser;

    public GetTransactionsByMonthUseCase(
        ITransactionRepository transactionRepository, ICurrentUserService currentUser)
    {
        _transactionRepository = transactionRepository;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<IEnumerable<TransactionFullResponse>>> ExecuteAsync(GetTransactionsByMonthRequest request)
    {
        var userId = _currentUser.UserId;

        var transactions = await _transactionRepository
            .GetByMonthAsync(userId, request.Year, request.Month);

        if (transactions is null || transactions.Count() <= 0)
            return ResultEntity<IEnumerable<TransactionFullResponse>>.Failure(MessageKeys.TransactionNotFound);

        var response = transactions.Select(t => new TransactionFullResponse(
            t.Id,
            t.UserId,
            t.Description,
            t.Amount,
            t.CodTypeTransaction,
            t.TransactionDate,
            t.Status,
            t.IdPaymentMethod,
            t.CategoryId,
            t.Installments,
            t.CategoryItemId,
            t.TransactionGroupId,
            t.InstallmentsText,
            t.TransactionGroup != null ? t.TransactionGroup.Type : string.Empty,
            t.CreatedAt
        ));

        return ResultEntity<IEnumerable<TransactionFullResponse>>.Success(
            response,
            MessageKeys.OperationSuccess
        );
    }
}


