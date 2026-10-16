using Application.Abstractions;
using Application.Abstractions.Common;
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
    private readonly ICurrentUserService _currentUser;

    public UpdateCategoryItemUseCase(ICategoryItemRepository categoryRepository, IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _categoryRepository = categoryRepository;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<CategoryItemResponse>> ExecuteAsync(UpdateCategoryItemRequest request)
    {
        var category = await _categoryRepository.GetByIdAsync(request.Id, _currentUser.UserId);
        if (category is null)
            return ResultEntity<CategoryItemResponse>.Failure(MessageKeys.SubcategoryNotFound);

        var duplicated = await _categoryRepository.GetByDescriptionAsync(request.Description, category.CategoryId);
        if (duplicated is not null)
            return ResultEntity<CategoryItemResponse>.Failure(MessageKeys.DescriptionAlreadyExists);


        category.UpdateDescription(request.Description);

        var result =  _categoryRepository.Update(category);
        if (!result)
            return ResultEntity<CategoryItemResponse>.Failure(MessageKeys.UpdateFailed);

        var uow = await _uow.CommitAsync();
        if (!uow)
            return ResultEntity<CategoryItemResponse>.Failure(MessageKeys.DataPersistenceFailed);

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
