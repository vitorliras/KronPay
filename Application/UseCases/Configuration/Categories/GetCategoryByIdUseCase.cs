using Application.Abstractions;
using Application.DTOs.Configuration.Categories;
using Application.DTOs.Configuration.Category;
using Domain.Interfaces;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Categories;

public sealed class GetCategoryByIdUseCase
    : IUseCase<GetCategoryByIdRequest, CategoryResponse>
{
    private readonly ICategoryRepository _repository;

    public GetCategoryByIdUseCase(ICategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<ResultEntity<CategoryResponse>> ExecuteAsync(GetCategoryByIdRequest request)
    {
        var category = await _repository.GetByIdAsync(request.Id, request.UserId);

        if (category is null)
            return ResultEntity<CategoryResponse>.Failure("", MessageKeys.CategoryNotFound);

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
