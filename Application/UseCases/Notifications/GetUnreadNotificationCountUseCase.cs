using Application.Abstractions;
using Application.Abstractions.Common;
using Domain.Interfaces.Notifications;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Notifications;

public sealed class GetUnreadNotificationCountUseCase
    : IUseCaseWithoutRequest<int>
{
    private readonly INotificationRepository _repository;
    private readonly ICurrentUserService _currentUser;

    public GetUnreadNotificationCountUseCase(
        INotificationRepository repository,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<int>> ExecuteAsync()
    {
        var count = await _repository.GetUnreadCountAsync(_currentUser.UserId);

        return ResultEntity<int>.Success(count, MessageKeys.OperationSuccess);
    }
}
