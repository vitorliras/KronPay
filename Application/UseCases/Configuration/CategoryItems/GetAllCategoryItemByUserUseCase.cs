using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Configuration.CategoryItems;
using Application.DTOs.Configuration.PaymentMethods;
using Domain.Interfaces;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.PaymentMethods;

public sealed class GetAllCategoryItemByUserUseCase
    : IUseCaseWithoutRequest< IEnumerable<CategoryItemResponse>>
{
    private readonly ICategoryItemRepository _repository;
    private readonly ICurrentUserService _currentUser;

    public GetAllCategoryItemByUserUseCase(ICategoryItemRepository repository, ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<IEnumerable<CategoryItemResponse>>> ExecuteAsync()
    {
        var userId = _currentUser.UserId;

        var categories = await _repository.GetAllByUserIdAsync(userId);

        var response = categories.Select(c =>
            new CategoryItemResponse(
                c.Id,
                c.Description,
                c.CategoryId,
                c.Active
            ));

        return ResultEntity<IEnumerable<CategoryItemResponse>>.Success(response, MessageKeys.OperationSuccess);
    }
}
