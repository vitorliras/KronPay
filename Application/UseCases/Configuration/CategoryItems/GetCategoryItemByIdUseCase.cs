using Application.Abstractions;
using Application.DTOs.Configuration.Categories;
using Application.DTOs.Configuration.Category;
using Application.DTOs.Configuration.CategoryItems;
using Domain.Interfaces;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Categories;

public sealed class GetCategoryItemByIdUseCase
    : IUseCase<GetCategoryItemByIdRequest, CategoryItemResponse>
{
    private readonly ICategoryItemRepository _repository;

    public GetCategoryItemByIdUseCase(ICategoryItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<ResultEntity<CategoryItemResponse>> ExecuteAsync(GetCategoryItemByIdRequest request)
    {
        var category = await _repository.GetByIdAsync(request.Id, request.CategoryId);

        if (category is null)
            return ResultEntity<CategoryItemResponse>.Failure("", MessageKeys.CategoryNotFound);

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
