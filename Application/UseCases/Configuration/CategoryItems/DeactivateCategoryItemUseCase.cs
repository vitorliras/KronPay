using Application.Abstractions;
using Application.DTOs.Configuration.Categories;
using Application.DTOs.Configuration.CategoryItems;
using Domain.Interfaces;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Categories;

public sealed class DeactivateCategoryItemUseCase
    : IUseCase<DeactivateCategoryItemRequest, Unit>
{
    private readonly ICategoryItemRepository _categoryRepository;
    private readonly IUnitOfWork _uow;

    public DeactivateCategoryItemUseCase(ICategoryItemRepository categoryRepository, IUnitOfWork uow)
    {
        _categoryRepository = categoryRepository;
        _uow = uow;
    }
    public async Task<ResultT<Unit>> ExecuteAsync(DeactivateCategoryItemRequest request)
    {
        var category = await _categoryRepository.GetByIdAsync(request.Id, request.CategoryId);
        if (category is null)
            return ResultT<Unit>.Failure("", MessageKeys.CategoryNotFound);

        category.Deactivate();
        var result = _categoryRepository.Update(category);
        if (!result)
            return ResultT<Unit>.Failure("", MessageKeys.OperationFailed);

        var uow = await _uow.CommitAsync();
        if (!uow)
            return ResultT<Unit>.Failure("", MessageKeys.OperationFailed);

        return ResultT<Unit>.Success(Unit.Value);
    }
}
