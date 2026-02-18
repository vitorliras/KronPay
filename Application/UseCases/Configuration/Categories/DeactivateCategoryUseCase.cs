using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Configuration.Categories;
using Domain.Entities;
using Domain.Interfaces;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Categories;

public sealed class DeactivateCategoryUseCase
    : IUseCase<DeactivateCategoryRequest, Unit>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICategoryItemRepository _categoryItemRepository;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DeactivateCategoryUseCase(ICategoryRepository categoryRepository, IUnitOfWork uow, ICategoryItemRepository categoryItemRepository, ICurrentUserService currentUser)
    {
        _categoryRepository = categoryRepository;
        _uow = uow;
        _categoryItemRepository = categoryItemRepository;
        _currentUser = currentUser;

    }
    public async Task<ResultEntity<Unit>> ExecuteAsync(DeactivateCategoryRequest request)
    {
        var userId = _currentUser.UserId;

        var categoryItems = await _categoryItemRepository.GetAllAsync(request.Id);

        if (categoryItems is not null || categoryItems.Count() > 0)
            return ResultEntity<Unit>.Failure("", MessageKeys.ExistsAnotherRegister);

        var categoryItem = await _categoryRepository.GetByIdAsync(request.Id, userId);
        if (categoryItem is null)
            return ResultEntity<Unit>.Failure("", MessageKeys.CategoryNotFound);

        categoryItem.Deactivate();
        var result = _categoryRepository.Update(categoryItem);
        if (!result)
            return ResultEntity<Unit>.Failure("", MessageKeys.OperationFailed);

        var uow = await _uow.CommitAsync();
        if (!uow)
            return ResultEntity<Unit>.Failure("", MessageKeys.OperationFailed);

        return ResultEntity<Unit>.Success(Unit.Value, MessageKeys.OperationSuccess);
    }
}
