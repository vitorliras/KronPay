using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Configuration;
using Domain.Interfaces;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Categories;

public sealed class DeactivateCategoryItemSelectUseCase
    : IUseCase<DeactivateCategoryItemSelectRequest, Unit>
{
    private readonly ICategoryItemRepository _repository;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DeactivateCategoryItemSelectUseCase(
        ICategoryItemRepository repository,
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<Unit>> ExecuteAsync(DeactivateCategoryItemSelectRequest request)
    {
        if (request is null || request.CategoryItems.Count == 0)
            return ResultEntity<Unit>.Failure(MessageKeys.OperationFailed);

        foreach (var item in request.CategoryItems)
        {
            var sub = await _repository.GetByIdAsync(item.Id, _currentUser.UserId);
            if (sub == null)
                return ResultEntity<Unit>.Failure(MessageKeys.SubcategoryNotFound);

            sub.Deactivate();

            var result = _repository.Update(sub);

            if (!result)
                return ResultEntity<Unit>.Failure(MessageKeys.UpdateFailed);
        }

        var committed = await _uow.CommitAsync();
        if (!committed)
            return ResultEntity<Unit>.Failure(MessageKeys.DataPersistenceFailed);

        return ResultEntity<Unit>.Success(Unit.Value, MessageKeys.OperationSuccess);
    }
}