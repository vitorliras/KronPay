using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Configuration.Categories;
using Domain.Interfaces;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Categories;

public sealed class GetAllCategoriesUseCase
    : IUseCaseWithoutRequest<IEnumerable<CategoryResponse>>
{
    private readonly ICategoryRepository _repository;
    private readonly ICurrentUserService _currentUser;

    public GetAllCategoriesUseCase(
        ICategoryRepository repository,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<IEnumerable<CategoryResponse>>> ExecuteAsync()
    {
        var userId = _currentUser.UserId;

        var categories = await _repository.GetAllAsync(userId);

        var response = categories.Select(c =>
            new CategoryResponse(
                c.Id,
                c.Description,
                c.CodTypeTransaction,
                c.Active
            ));

        return ResultEntity<IEnumerable<CategoryResponse>>
            .Success(response, MessageKeys.OperationSuccess);
    }
}
