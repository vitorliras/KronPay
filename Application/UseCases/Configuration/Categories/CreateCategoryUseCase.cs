using Application.Abstractions;
using Application.DTOs.Configuration.Categories;
using Application.DTOs.Configuration.Category;
using Domain.Entities.Configuration;
using Domain.Interfaces;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Categories;

public sealed class CreateCategoryUseCase
    : IUseCase<CreateCategoryRequest, CategoryResponse>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _uow;

    public CreateCategoryUseCase(ICategoryRepository categoryrepository, IUnitOfWork uow)
    {
        _categoryRepository = categoryrepository;
        _uow = uow;
    }

    public async Task<ResultT<CategoryResponse>> ExecuteAsync(CreateCategoryRequest request)
    {
        var category = await _categoryRepository.GetByDescriptionAsync(request.Description, request.UserId);

        if (category is not null)
            return ResultT<CategoryResponse>.Failure("", MessageKeys.DescriptionAlreadyExists);

         category = new Category(
            request.UserId,
            request.Description,
            request.CodTypeTransaction
        );
        
        var result = await _categoryRepository.AddAsync(category);
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
