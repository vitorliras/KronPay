using Application.Abstractions;
using Application.DTOs.Configuration.Categories;
using Application.DTOs.Configuration.CategoryItems;
using Domain.Interfaces;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Categories;

public sealed class UpdateCategoryItemUseCase
    : IUseCase<UpdateCategoryItemRequest, CategoryItemResponse>
{
    private readonly ICategoryItemRepository _categoryRepository;
    private readonly IUnitOfWork _uow;

    public UpdateCategoryItemUseCase(ICategoryItemRepository categoryRepository, IUnitOfWork uow)
    {
        _categoryRepository = categoryRepository;
        _uow = uow;
    }

    public async Task<ResultEntity<CategoryItemResponse>> ExecuteAsync(UpdateCategoryItemRequest request)
    {
        var category = await _categoryRepository.GetByDescriptionAsync(request.Description, request.CategoryId);

        if (category is not null)
            return ResultEntity<CategoryItemResponse>.Failure("", MessageKeys.DescriptionAlreadyExists);

         category = await _categoryRepository.GetByIdAsync(request.Id, request.CategoryId);
        if (category is null)
            return ResultEntity<CategoryItemResponse>.Failure("", MessageKeys.CategoryNotFound);


        category.UpdateDescription(request.Description);

        var result =  _categoryRepository.Update(category);
        if (!result)
            return ResultEntity<CategoryItemResponse>.Failure("", MessageKeys.OperationFailed);

        var uow = await _uow.CommitAsync();
        if (!uow)
            return ResultEntity<CategoryItemResponse>.Failure("", MessageKeys.OperationFailed);

        return ResultEntity<CategoryItemResponse>.Success(
            new CategoryItemResponse(
                category.Id,
                category.Description,
                category.CategoryId,
                category.Active
            ), MessageKeys.OperationSuccess
        );
    }
}
