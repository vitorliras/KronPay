using Application.Abstractions;
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

    public GetAllCategoryItemsUseCase(ICategoryItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<ResultEntity<IEnumerable<CategoryItemResponse>>> ExecuteAsync(GetAllCategoryItemsRequest request)
    {
        if(request.CategoryId <= 0)
            return ResultEntity<IEnumerable<CategoryItemResponse>>.Failure(MessageKeys.ItemItemNotFound);

        var categories = await _repository.GetAllAsync(request.CategoryId);

        var response = categories.Select(c =>
            new CategoryItemResponse(
                c.Id,
                c.Description,
                c.CategoryId,
                c.Active
            ));

        if(!response.Any())
            return ResultEntity<IEnumerable<CategoryItemResponse>>.Failure(MessageKeys.ItemItemNotFound);

        return ResultEntity<IEnumerable<CategoryItemResponse>>.Success(response, MessageKeys.OperationSuccess);
    }

}
