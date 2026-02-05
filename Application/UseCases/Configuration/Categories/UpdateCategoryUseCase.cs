using Application.Abstractions;
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

    public UpdateCategoryUseCase(ICategoryRepository categoryRepository, IUnitOfWork uow)
    {
        _categoryRepository = categoryRepository;
        _uow = uow;
    }

    public async Task<ResultT<CategoryResponse>> ExecuteAsync(UpdateCategoryRequest request)
    {
        var category = await _categoryRepository.GetByDescriptionAsync(request.Description, request.UserId);

        if (category is not null)
            return ResultT<CategoryResponse>.Failure("", MessageKeys.DescriptionAlreadyExists);

         category = await _categoryRepository.GetByIdAsync(request.Id, request.UserId);

        if (category is null)
            return ResultT<CategoryResponse>.Failure("", MessageKeys.CategoryNotFound);

        category.UpdateDescription(request.Description);

        var result =  _categoryRepository.Update(category);
        if (!result)
            return ResultT<CategoryResponse>.Failure("", MessageKeys.OperationFailed);

        var uow = await _uow.CommitAsync();
        if (!uow)
            return ResultT<CategoryResponse>.Failure("", MessageKeys.OperationFailed);

        return ResultT<CategoryResponse>.Success(
            new CategoryResponse(
                category.Id,
                category.Description,
                category.CodTypeTransaction,
                category.Active
            )
        );
    }
}
