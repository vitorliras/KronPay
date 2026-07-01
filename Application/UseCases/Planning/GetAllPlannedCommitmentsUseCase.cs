using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Planning;
using Application.Planning;
using Domain.Interfaces.Planning;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Planning;

public sealed class GetAllPlannedCommitmentsUseCase
    : IUseCaseWithoutRequest<IEnumerable<PlannedCommitmentResponse>>
{
    private readonly IPlannedCommitmentRepository _repository;
    private readonly ICurrentUserService _currentUser;

    public GetAllPlannedCommitmentsUseCase(
        IPlannedCommitmentRepository repository,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<IEnumerable<PlannedCommitmentResponse>>> ExecuteAsync()
    {
        var userId = _currentUser.UserId;

        var commitments = await _repository.GetByUserAsync(userId);

        var response = commitments.Select(PlannedCommitmentMapper.ToResponse);

        return ResultEntity<IEnumerable<PlannedCommitmentResponse>>
            .Success(response, MessageKeys.OperationSuccess);
    }
}
