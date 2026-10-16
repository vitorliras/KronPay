using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Configuration.Categories;
using Application.DTOs.Configuration.CategoryItems;
using Domain.Interfaces;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Categories;

public sealed class DeactivateCategoryItemUseCase
    : IUseCase<DeactivateCategoryItemRequest, Unit>
{
    private readonly ICategoryItemRepository _categoryRepository;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DeactivateCategoryItemUseCase(ICategoryItemRepository categoryRepository, IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _categoryRepository = categoryRepository;
        _uow = uow;
        _currentUser = currentUser;
    }
    public async Task<ResultEntity<Unit>> ExecuteAsync(DeactivateCategoryItemRequest request)
    {
        var category = await _categoryRepository.GetByIdAsync(request.Id, _currentUser.UserId);
        if (category is null)
            return ResultEntity<Unit>.Failure(MessageKeys.SubcategoryNotFound);

        category.Deactivate();
        var result = _categoryRepository.Update(category);
        if (!result)
            return ResultEntity<Unit>.Failure(MessageKeys.UpdateFailed);

        var uow = await _uow.CommitAsync();
        if (!uow)
            return ResultEntity<Unit>.Failure(MessageKeys.DataPersistenceFailed);

        return ResultEntity<Unit>.Success(Unit.Value, MessageKeys.OperationSuccess);
    }
}
