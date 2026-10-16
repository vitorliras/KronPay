using Application.Abstractions;
using Application.Abstractions.Common;
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
    private readonly ICurrentUserService _currentUser;

    public GetCategoryItemByIdUseCase(ICategoryItemRepository repository, ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<CategoryItemResponse>> ExecuteAsync(GetCategoryItemByIdRequest request)
    {
        var category = await _repository.GetByIdAsync(request.Id, _currentUser.UserId);

        if (category is null)
            return ResultEntity<CategoryItemResponse>.Failure(MessageKeys.SubcategoryNotFound);

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
