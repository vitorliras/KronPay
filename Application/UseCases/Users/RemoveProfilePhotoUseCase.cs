using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Users;
using Domain.Interfaces;
using Domain.Interfaces.Users;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Users;

public sealed class RemoveProfilePhotoUseCase : IUseCaseWithoutRequest<ProfilePhotoResponse>
{
    private readonly IUserProfilePhotoRepository _repository;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public RemoveProfilePhotoUseCase(
        IUserProfilePhotoRepository repository,
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<ProfilePhotoResponse>> ExecuteAsync()
    {
        var userId = _currentUser.UserId;

        var existing = await _repository.GetByUserIdAsync(userId);
        if (existing is null)
            return ResultEntity<ProfilePhotoResponse>.Success(
                new ProfilePhotoResponse(false), MessageKeys.ProfilePhotoRemoved);

        if (!await _repository.DeleteAsync(existing))
            return ResultEntity<ProfilePhotoResponse>.Failure(MessageKeys.DeleteFailed);

        if (!await _uow.CommitAsync())
            return ResultEntity<ProfilePhotoResponse>.Failure(MessageKeys.DataPersistenceFailed);

        return ResultEntity<ProfilePhotoResponse>.Success(
            new ProfilePhotoResponse(false), MessageKeys.ProfilePhotoRemoved);
    }
}
