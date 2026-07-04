using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Users;
using Domain.Interfaces.Users;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Users;

public sealed class GetProfilePhotoUseCase : IUseCaseWithoutRequest<ProfilePhotoContentResponse>
{
    private readonly IUserProfilePhotoRepository _repository;
    private readonly ICurrentUserService _currentUser;

    public GetProfilePhotoUseCase(
        IUserProfilePhotoRepository repository,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<ProfilePhotoContentResponse>> ExecuteAsync()
    {
        var userId = _currentUser.UserId;

        var photo = await _repository.GetByUserIdAsync(userId);
        if (photo is null)
            return ResultEntity<ProfilePhotoContentResponse>.Failure(MessageKeys.ItemNotFound);

        return ResultEntity<ProfilePhotoContentResponse>.Success(
            new ProfilePhotoContentResponse(photo.Photo, photo.ContentType));
    }
}
