using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Configuration;
using Application.DTOs.Configuration.Categories;
using Application.DTOs.Configuration.CategoryItems;
using Application.DTOs.Transactions;
using Domain.Interfaces;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Categories;

public sealed class DeactivateCategorySelectUseCase
    : IUseCase<DeactivateCategorySelectRequest, Unit>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICategoryItemRepository _categoryItemRepository;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DeactivateCategorySelectUseCase(ICategoryRepository categoryRepository, IUnitOfWork uow, ICategoryItemRepository categoryItemRepository, ICurrentUserService currentUser)
    {
        _categoryRepository = categoryRepository;
        _uow = uow;
        _categoryItemRepository = categoryItemRepository;
        _currentUser = currentUser;

    }
    public async Task<ResultEntity<Unit>> ExecuteAsync(DeactivateCategorySelectRequest request)
    {
        var userId = _currentUser.UserId;

        foreach (var item in request.Categories)
        {

            var categoryItems = await _categoryItemRepository.GetAllAsync(item.Id);

            if (categoryItems is not null && categoryItems.Count() > 0)
                return ResultEntity<Unit>.Failure(MessageKeys.ExistsAnotherRegister);

            var category = await _categoryRepository.GetByIdAsync(item.Id, userId);
            if (category is null)
                return ResultEntity<Unit>.Failure(MessageKeys.CategoryNotFound);

            category.Deactivate();
            var result = _categoryRepository.Update(category);

            if (!result)
                return ResultEntity<Unit>.Failure(MessageKeys.OperationFailed);
        }

        var uow = await _uow.CommitAsync();
        if (!uow)
            return ResultEntity<Unit>.Failure(MessageKeys.OperationFailed);

        return ResultEntity<Unit>.Success(Unit.Value, MessageKeys.OperationSuccess);
    }
}
