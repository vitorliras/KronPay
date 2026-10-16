using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Configuration.Categories;
using Application.DTOs.Configuration.Category;
using Application.DTOs.Configuration.CategoryItems;
using Domain.Interfaces;
using Shared.Localization;
using Shared.Results;
using System.Collections.Generic;

namespace Application.UseCases.Categories;

public sealed class GetAllCategoryItemsUseCase
    : IUseCase<GetAllCategoryItemsRequest, IEnumerable<CategoryItemResponse>>
{
    private readonly ICategoryItemRepository _repository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICurrentUserService _currentUser;

    public GetAllCategoryItemsUseCase(
        ICategoryItemRepository repository,
        ICategoryRepository categoryRepository,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _categoryRepository = categoryRepository;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<IEnumerable<CategoryItemResponse>>> ExecuteAsync(GetAllCategoryItemsRequest request)
    {
        if(request.CategoryId <= 0)
            return ResultEntity<IEnumerable<CategoryItemResponse>>.Failure(MessageKeys.SubcategoryNotFound);

        var parentCategory = await _categoryRepository.GetByIdAsync(request.CategoryId, _currentUser.UserId);
        if (parentCategory is null)
            return ResultEntity<IEnumerable<CategoryItemResponse>>.Failure(MessageKeys.SubcategoryNotFound);

        var categories = await _repository.GetAllAsync(request.CategoryId);

        var response = categories.Select(c =>
            new CategoryItemResponse(
                c.Id,
                c.Description,
                c.CategoryId,
                c.Active
            ));

        if(!response.Any())
            return ResultEntity<IEnumerable<CategoryItemResponse>>.Failure(MessageKeys.SubcategoryNotFound);

        return ResultEntity<IEnumerable<CategoryItemResponse>>.Success(response, MessageKeys.OperationSuccess);
    }

}
