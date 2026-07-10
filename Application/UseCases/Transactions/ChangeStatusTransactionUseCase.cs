using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Transactions;
using Application.Notifications;
using Domain.Enums.Gamification;
using Domain.Interfaces;
using Domain.Interfaces.Transactions;
using Domain.Services.Gamification;
using Microsoft.Extensions.Logging;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Transactions;

public sealed class ChangeStatusTransactionUseCase
    : IUseCase<ChangeStatusTransactionRequest, TransactionResponse>
{

    private readonly ICurrentUserService _currentUser;
    private readonly ITransactionRepository _transactionRepository;
    private readonly INotificationService _notificationService;
    private readonly IGamificationService _gamificationService;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<ChangeStatusTransactionUseCase> _logger;

    public ChangeStatusTransactionUseCase(
        ITransactionRepository transactionRepository,
        INotificationService notificationService,
        IGamificationService gamificationService,
        IUnitOfWork uow,
        ICurrentUserService currentUser,
        ILogger<ChangeStatusTransactionUseCase> logger)
    {
        _transactionRepository = transactionRepository;
        _notificationService = notificationService;
        _gamificationService = gamificationService;
        _uow = uow;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<ResultEntity<TransactionResponse>> ExecuteAsync(ChangeStatusTransactionRequest request)
    {
        var userId = _currentUser.UserId;

        var transaction = await _transactionRepository
            .GetByIdAsync(request.Id, userId);

        if (transaction is null)
            return ResultEntity<TransactionResponse>.Failure(MessageKeys.TransactionNotFound);

        else
        {
            if (request.Status.Equals("P"))
            {
                transaction.Paid();
                await _notificationService.ResolveByRelatedEntityAsync(userId, "Transaction", transaction.Id);
            }
            if (request.Status.Equals("C"))  transaction.Cancel();
            if (request.Status.Equals("O"))  transaction.Open();
            if (request.Status.Equals("E"))  transaction.Expired();

            if (!await _transactionRepository.UpdateAsync(transaction))
                return ResultEntity<TransactionResponse>.Failure(MessageKeys.UpdateFailed);
        }

        if (!await _uow.CommitAsync())
            return ResultEntity<TransactionResponse>.Failure(MessageKeys.DataPersistenceFailed);

        await TriggerGamificationBestEffort(userId);

        return ResultEntity<TransactionResponse>.Success(
            new TransactionResponse(
                transaction.Id,
                transaction.Description,
                transaction.TransactionGroupId ?? 0,
                1
            ), MessageKeys.OperationSuccess
        );
    }

    private async Task TriggerGamificationBestEffort(int userId)
    {
        try
        {
            await _gamificationService.TriggerInstantEvaluationAsync(userId, MissionEventType.TransactionMonthSurplus, null);

            if (!await _uow.CommitAsync())
                _logger.LogError(
                    "Falha ao persistir a avaliação instantânea de gamificação para o usuário {UserId}.", userId);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Falha inesperada ao avaliar gamificação instantânea para o usuário {UserId}.", userId);
        }
    }
}
