using Application.Abstractions;
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
    private readonly IUnitOfWork _uow;

    public CreateCategoryItemUseCase(ICategoryItemRepository categoryrepository, IUnitOfWork uow)
    {
        _categoryItemRepository = categoryrepository;
        _uow = uow;
    }

    public async Task<ResultEntity<CategoryItemResponse>> ExecuteAsync(CreateCategoryItemRequest request)
    {
        var categoryItem = await _categoryItemRepository.GetByDescriptionAsync(request.Description, request.CategoryId);

        if (categoryItem is not null)
            return ResultEntity<CategoryItemResponse>.Failure("", MessageKeys.DescriptionAlreadyExists);

        categoryItem = new CategoryItem(request.CategoryId, request.Description);

        var result = await _categoryItemRepository.AddAsync(categoryItem);
        if (!result)
            return ResultEntity<CategoryItemResponse>.Failure("", MessageKeys.OperationFailed);

        var uow = await _uow.CommitAsync();
        if (!uow)
            return ResultEntity<CategoryItemResponse>.Failure("", MessageKeys.OperationFailed);

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
