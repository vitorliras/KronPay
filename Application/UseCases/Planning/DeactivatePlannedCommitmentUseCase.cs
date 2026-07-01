using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Planning;
using Domain.Interfaces;
using Domain.Interfaces.Planning;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Planning;

public sealed class DeactivatePlannedCommitmentUseCase
    : IUseCase<DeactivatePlannedCommitmentRequest, Unit>
{
    private readonly IPlannedCommitmentRepository _repository;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DeactivatePlannedCommitmentUseCase(
        IPlannedCommitmentRepository repository,
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<Unit>> ExecuteAsync(DeactivatePlannedCommitmentRequest request)
    {
        var userId = _currentUser.UserId;

        var commitment = await _repository.GetByIdAsync(request.Id, userId);
        if (commitment is null || !commitment.Active)
            return ResultEntity<Unit>.Failure(MessageKeys.PlannedCommitmentNotFound);

        commitment.Deactivate();

        if (!_repository.Update(commitment))
            return ResultEntity<Unit>.Failure(MessageKeys.UpdateFailed);

        if (!await _uow.CommitAsync())
            return ResultEntity<Unit>.Failure(MessageKeys.DataPersistenceFailed);

        return ResultEntity<Unit>.Success(Unit.Value, MessageKeys.OperationSuccess);
    }
}
