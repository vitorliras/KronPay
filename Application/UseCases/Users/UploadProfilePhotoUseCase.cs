using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Users;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Interfaces.Users;
using Domain.Services.Users;
using KronPay.Domain.Entities.Users;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Users;

public sealed class UploadProfilePhotoUseCase
    : IUseCase<UploadProfilePhotoRequest, ProfilePhotoResponse>
{
    private readonly IUserProfilePhotoRepository _repository;
    private readonly IProfilePhotoProcessor _processor;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public UploadProfilePhotoUseCase(
        IUserProfilePhotoRepository repository,
        IProfilePhotoProcessor processor,
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _processor = processor;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<ProfilePhotoResponse>> ExecuteAsync(UploadProfilePhotoRequest request)
    {
        var userId = _currentUser.UserId;

        ProcessedPhoto processed;
        try
        {
            processed = _processor.Process(request.Content);
        }
        catch (DomainException ex)
        {
            return ResultEntity<ProfilePhotoResponse>.Failure(ex.Message);
        }

        var existing = await _repository.GetByUserIdAsync(userId);

        if (existing is null)
        {
            var photo = new UserProfilePhoto(userId, processed.Bytes, processed.ContentType);

            if (!await _repository.AddAsync(photo))
                return ResultEntity<ProfilePhotoResponse>.Failure(MessageKeys.InsertFalied);
        }
        else
        {
            existing.Replace(processed.Bytes, processed.ContentType);

            if (!_repository.Update(existing))
                return ResultEntity<ProfilePhotoResponse>.Failure(MessageKeys.UpdateFailed);
        }

        if (!await _uow.CommitAsync())
            return ResultEntity<ProfilePhotoResponse>.Failure(MessageKeys.DataPersistenceFailed);

        return ResultEntity<ProfilePhotoResponse>.Success(
            new ProfilePhotoResponse(true), MessageKeys.ProfilePhotoUpdated);
    }
}
