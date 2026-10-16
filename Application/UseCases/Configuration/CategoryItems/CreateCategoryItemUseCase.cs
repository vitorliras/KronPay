using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Configuration.Categories;
using Application.DTOs.Configuration.Category;
using Application.DTOs.Configuration.CategoryItems;
using Domain.Entities;
using Domain.Entities.Configuration;
using Domain.Interfaces;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Categories;

public sealed class CreateCategoryItemUseCase
    : IUseCase<CreateCategoryItemRequest, CategoryItemResponse>
{
    private readonly ICategoryItemRepository _categoryItemRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreateCategoryItemUseCase(
        ICategoryItemRepository categoryItemRepository,
        ICategoryRepository categoryRepository,
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _categoryItemRepository = categoryItemRepository;
        _categoryRepository = categoryRepository;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<CategoryItemResponse>> ExecuteAsync(CreateCategoryItemRequest request)
    {
        var parentCategory = await _categoryRepository.GetByIdAsync(request.CategoryId, _currentUser.UserId);
        if (parentCategory is null)
            return ResultEntity<CategoryItemResponse>.Failure(MessageKeys.CategoryNotFound);

        var categoryItem = await _categoryItemRepository.GetByDescriptionAsync(request.Description, request.CategoryId);

        if (categoryItem is not null)
            return ResultEntity<CategoryItemResponse>.Failure(MessageKeys.DescriptionAlreadyExists);

        categoryItem = new CategoryItem(request.CategoryId, request.Description);

        var result = await _categoryItemRepository.AddAsync(categoryItem);
        if (!result)
            return ResultEntity<CategoryItemResponse>.Failure(MessageKeys.InsertFalied);

        var uow = await _uow.CommitAsync();
        if (!uow)
            return ResultEntity<CategoryItemResponse>.Failure(MessageKeys.DataPersistenceFailed);

        return ResultEntity<CategoryItemResponse>.Success(
            new CategoryItemResponse(
                categoryItem.Id,
                categoryItem.Description,
                categoryItem.CategoryId,
                categoryItem.Active
            ), MessageKeys.OperationSuccess
        );
    }
}
