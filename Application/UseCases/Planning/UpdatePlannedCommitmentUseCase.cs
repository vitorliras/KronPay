using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Planning;
using Application.Planning;
using Domain.Interfaces;
using Domain.Interfaces.Planning;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Planning;

public sealed class UpdatePlannedCommitmentUseCase
    : IUseCase<UpdatePlannedCommitmentRequest, PlannedCommitmentResponse>
{
    private readonly IPlannedCommitmentRepository _repository;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public UpdatePlannedCommitmentUseCase(
        IPlannedCommitmentRepository repository,
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<PlannedCommitmentResponse>> ExecuteAsync(UpdatePlannedCommitmentRequest request)
    {
        var userId = _currentUser.UserId;

        var commitment = await _repository.GetByIdAsync(request.Id, userId);
        if (commitment is null || !commitment.Active)
            return ResultEntity<PlannedCommitmentResponse>.Failure(MessageKeys.PlannedCommitmentNotFound);

        commitment.UpdateDetails(
            request.Description,
            request.Amount,
            request.Direction,
            request.Periodicity,
            request.StartDate,
            request.EndDate,
            request.CategoryId);

        if (!_repository.Update(commitment))
            return ResultEntity<PlannedCommitmentResponse>.Failure(MessageKeys.UpdateFailed);

        if (!await _uow.CommitAsync())
            return ResultEntity<PlannedCommitmentResponse>.Failure(MessageKeys.DataPersistenceFailed);

        return ResultEntity<PlannedCommitmentResponse>.Success(
            PlannedCommitmentMapper.ToResponse(commitment),
            MessageKeys.OperationSuccess);
    }
}
