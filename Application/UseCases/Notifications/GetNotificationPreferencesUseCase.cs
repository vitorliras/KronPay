using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Notifications;
using Domain.Interfaces.Notifications;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Notifications;

public sealed class GetNotificationPreferencesUseCase
    : IUseCaseWithoutRequest<NotificationPreferenceResponse>
{
    private readonly INotificationPreferenceRepository _repository;
    private readonly ICurrentUserService _currentUser;

    public GetNotificationPreferencesUseCase(
        INotificationPreferenceRepository repository,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<NotificationPreferenceResponse>> ExecuteAsync()
    {
        var preference = await _repository.GetByUserIdAsync(_currentUser.UserId);

        // Usuários criados antes desta feature não têm linha de preferência ainda
        // (sem migração de backfill) — cair nos defaults em vez de falhar.
        var response = preference is null
            ? new NotificationPreferenceResponse(true, true, false)
            : new NotificationPreferenceResponse(
                preference.EmailOnCritical,
                preference.EmailOnImportant,
                preference.EmailOnInformative);

        return ResultEntity<NotificationPreferenceResponse>.Success(response, MessageKeys.OperationSuccess);
    }
}
