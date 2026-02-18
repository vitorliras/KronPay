using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Configuration.Categories;
using Application.DTOs.Configuration.Category;
using Domain.Interfaces;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Categories;

public sealed class UpdateCategoryUseCase
    : IUseCase<UpdateCategoryRequest, CategoryResponse>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public UpdateCategoryUseCase(ICategoryRepository categoryRepository, IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _categoryRepository = categoryRepository;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<CategoryResponse>> ExecuteAsync(UpdateCategoryRequest request)
    {
        var userId = _currentUser.UserId;

        var category = await _categoryRepository.GetByDescriptionAsync(request.Description, userId);

        if (category is not null)
            return ResultEntity<CategoryResponse>.Failure("", MessageKeys.DescriptionAlreadyExists);

         category = await _categoryRepository.GetByIdAsync(request.Id, userId);

        if (category is null)
            return ResultEntity<CategoryResponse>.Failure("", MessageKeys.CategoryNotFound);

        category.UpdateDescription(request.Description);

        var result =  _categoryRepository.Update(category);
        if (!result)
            return ResultEntity<CategoryResponse>.Failure("", MessageKeys.OperationFailed);

        var uow = await _uow.CommitAsync();
        if (!uow)
            return ResultEntity<CategoryResponse>.Failure("", MessageKeys.OperationFailed);

        return ResultEntity<CategoryResponse>.Success(
            new CategoryResponse(
                category.Id,
                category.Description,
                category.CodTypeTransaction,
                category.Active
            ), MessageKeys.OperationSuccess
        );
    }
}
