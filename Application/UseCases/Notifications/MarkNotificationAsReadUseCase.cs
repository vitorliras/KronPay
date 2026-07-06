using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Notifications;
using Application.Notifications;
using Domain.Interfaces;
using Domain.Interfaces.Notifications;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Notifications;

public sealed class MarkNotificationAsReadUseCase
    : IUseCase<MarkNotificationAsReadRequest, NotificationResponse>
{
    private readonly INotificationRepository _repository;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public MarkNotificationAsReadUseCase(
        INotificationRepository repository,
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<NotificationResponse>> ExecuteAsync(MarkNotificationAsReadRequest request)
    {
        var userId = _currentUser.UserId;

        var notification = await _repository.GetByIdAsync(request.Id, userId);
        if (notification is null)
            return ResultEntity<NotificationResponse>.Failure(MessageKeys.NotificationNotFound);

        notification.MarkAsRead();

        if (NotificationTypeCatalog.IsEventBased(notification.Type))
            notification.Archive();

        if (!_repository.Update(notification))
            return ResultEntity<NotificationResponse>.Failure(MessageKeys.UpdateFailed);

        if (!await _uow.CommitAsync())
            return ResultEntity<NotificationResponse>.Failure(MessageKeys.DataPersistenceFailed);

        return ResultEntity<NotificationResponse>.Success(
            NotificationResponseMapper.ToResponse(notification),
            MessageKeys.OperationSuccess);
    }
}
