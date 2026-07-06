using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Notifications;
using Domain.Interfaces.Notifications;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Notifications;

public sealed class GetCriticalUnresolvedNotificationsUseCase
    : IUseCaseWithoutRequest<IReadOnlyList<NotificationResponse>>
{
    private readonly INotificationRepository _repository;
    private readonly ICurrentUserService _currentUser;

    public GetCriticalUnresolvedNotificationsUseCase(
        INotificationRepository repository,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<IReadOnlyList<NotificationResponse>>> ExecuteAsync()
    {
        var notifications = await _repository.GetUnresolvedCriticalByUserAsync(_currentUser.UserId);

        var response = notifications
            .Select(NotificationResponseMapper.ToResponse)
            .ToList();

        return ResultEntity<IReadOnlyList<NotificationResponse>>
            .Success(response, MessageKeys.OperationSuccess);
    }
}
