using Application.Abstractions;
using Application.DTOs.Configuration.Categories;
using Application.DTOs.Configuration.Category;
using Application.DTOs.Configuration.CategoryItems;
using Domain.Interfaces;
using Shared.Results;

namespace Application.UseCases.Categories;

public sealed class GetAllCategoryItemsUseCase
    : IUseCase<GetAllCategoryItemsRequest, IEnumerable<CategoryItemResponse>>
{
    private readonly ICategoryItemRepository _repository;

    public GetAllCategoryItemsUseCase(ICategoryItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<ResultT<IEnumerable<CategoryItemResponse>>> ExecuteAsync(GetAllCategoryItemsRequest request)
    {
        var categories = await _repository.GetAllAsync(request.CategoryId);

        var response = categories.Select(c =>
            new CategoryItemResponse(
                c.Id,
                c.Description,
                c.CategoryId,
                c.Active
            ));

        return ResultT<IEnumerable<CategoryItemResponse>>.Success(response);
    }

}
