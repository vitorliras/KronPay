using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Notifications;
using Domain.Entities.Notifications;
using Domain.Interfaces;
using Domain.Interfaces.Notifications;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Notifications;

public sealed class UpdateNotificationPreferencesUseCase
    : IUseCase<UpdateNotificationPreferencesRequest, NotificationPreferenceResponse>
{
    private readonly INotificationPreferenceRepository _repository;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public UpdateNotificationPreferencesUseCase(
        INotificationPreferenceRepository repository,
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<NotificationPreferenceResponse>> ExecuteAsync(
        UpdateNotificationPreferencesRequest request)
    {
        var userId = _currentUser.UserId;

        var preference = await _repository.GetByUserIdAsync(userId);

        bool persisted;
        if (preference is null)
        {
            // Upsert: usuários criados antes desta feature não têm linha ainda.
            preference = new NotificationPreference(
                userId,
                request.EmailOnCritical,
                request.EmailOnImportant,
                request.EmailOnInformative);

            persisted = await _repository.AddAsync(preference);
        }
        else
        {
            preference.UpdatePreferences(
                request.EmailOnCritical,
                request.EmailOnImportant,
                request.EmailOnInformative);

            persisted = _repository.Update(preference);
        }

        if (!persisted)
            return ResultEntity<NotificationPreferenceResponse>.Failure(MessageKeys.UpdateFailed);

        if (!await _uow.CommitAsync())
            return ResultEntity<NotificationPreferenceResponse>.Failure(MessageKeys.DataPersistenceFailed);

        return ResultEntity<NotificationPreferenceResponse>.Success(
            new NotificationPreferenceResponse(
                preference.EmailOnCritical,
                preference.EmailOnImportant,
                preference.EmailOnInformative),
            MessageKeys.OperationSuccess);
    }
}
