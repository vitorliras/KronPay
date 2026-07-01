using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Planning;
using Application.Planning;
using Domain.Entities.Planning;
using Domain.Interfaces;
using Domain.Interfaces.Planning;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Planning;

public sealed class CreatePlannedCommitmentUseCase
    : IUseCase<CreatePlannedCommitmentRequest, PlannedCommitmentResponse>
{
    private readonly IPlannedCommitmentRepository _repository;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreatePlannedCommitmentUseCase(
        IPlannedCommitmentRepository repository,
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<PlannedCommitmentResponse>> ExecuteAsync(CreatePlannedCommitmentRequest request)
    {
        var userId = _currentUser.UserId;

        var commitment = new PlannedCommitment(
            userId,
            request.Description,
            request.Amount,
            request.Direction,
            request.Periodicity,
            request.StartDate,
            request.EndDate,
            request.CategoryId);

        if (!await _repository.AddAsync(commitment))
            return ResultEntity<PlannedCommitmentResponse>.Failure(MessageKeys.InsertFalied);

        if (!await _uow.CommitAsync())
            return ResultEntity<PlannedCommitmentResponse>.Failure(MessageKeys.DataPersistenceFailed);

        return ResultEntity<PlannedCommitmentResponse>.Success(
            PlannedCommitmentMapper.ToResponse(commitment),
            MessageKeys.OperationSuccess);
    }
}
