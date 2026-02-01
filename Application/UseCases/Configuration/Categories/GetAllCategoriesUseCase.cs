using Application.Abstractions;
using Application.DTOs.Configuration.Categories;
using Application.DTOs.Configuration.Category;
using Domain.Interfaces;
using Shared.Results;

namespace Application.UseCases.Categories;

public sealed class GetAllCategoriesUseCase
    : IUseCase<GetAllCategoriesRequest, IEnumerable<CategoryResponse>>
{
    private readonly ICategoryRepository _repository;

    public GetAllCategoriesUseCase(ICategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<ResultT<IEnumerable<CategoryResponse>>> ExecuteAsync(GetAllCategoriesRequest request)
    {
        var categories = await _repository.GetAllAsync(request.UserId);

        var response = categories.Select(c =>
            new CategoryResponse(
                c.Id,
                c.Description,
                c.CodTypeTransaction,
                c.Active
            ));

        return ResultT<IEnumerable<CategoryResponse>>.Success(response);
    }
}
