using Application.Abstractions;
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

    public DeactivateCategoryItemSelectUseCase(
        ICategoryItemRepository repository,
        IUnitOfWork uow)
    {
        _repository = repository;
        _uow = uow;
    }

    public async Task<ResultEntity<Unit>> ExecuteAsync(DeactivateCategoryItemSelectRequest request)
    {
        if (request is null || request.CategoryItems.Count == 0)
            return ResultEntity<Unit>.Failure(MessageKeys.OperationFailed);

        foreach (var item in request.CategoryItems)
        {
            var sub = await _repository.GetByIdAsync(item.Id);
            if (sub == null)
                return ResultEntity<Unit>.Failure(MessageKeys.CategoryNotFound);

            sub.Deactivate();

            var result = _repository.Update(sub);

            if (!result)
                return ResultEntity<Unit>.Failure(MessageKeys.OperationFailed);
        }

        var committed = await _uow.CommitAsync();
        if (!committed)
            return ResultEntity<Unit>.Failure(MessageKeys.OperationFailed);

        return ResultEntity<Unit>.Success(Unit.Value, MessageKeys.OperationSuccess);
    }
}