using Application.Abstractions;
using Application.DTOs.Configuration.Categories;
using Domain.Entities;
using Domain.Interfaces;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Categories;

public sealed class DeactivateCategoryUseCase
    : IUseCase<DeactivateCategoryRequest, Unit>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICategoryItemRepository _categoryItemRepository;
    private readonly IUnitOfWork _uow;

    public DeactivateCategoryUseCase(ICategoryRepository categoryRepository, IUnitOfWork uow, ICategoryItemRepository categoryItemRepository)
    {
        _categoryRepository = categoryRepository;
        _uow = uow;
        _categoryItemRepository = categoryItemRepository;
    }
    public async Task<ResultT<Unit>> ExecuteAsync(DeactivateCategoryRequest request)
    {
        var categoryItems = await _categoryItemRepository.GetAllAsync(request.Id);

        if (categoryItems is not null || categoryItems.Count() > 0)
            return ResultT<Unit>.Failure(MessageKeys.ExistsAnotherRegister);

        var categoryItem = await _categoryRepository.GetByIdAsync(request.Id, request.UserId);
        if (categoryItem is null)
            return ResultT<Unit>.Failure(MessageKeys.CategoryNotFound);

        categoryItem.Deactivate();
        var result = _categoryRepository.UpdateAsync(categoryItem);
        if (!result)
            return ResultT<Unit>.Failure(MessageKeys.OperationFailed);

        var uow = await _uow.CommitAsync();
        if (!uow)
            return ResultT<Unit>.Failure(MessageKeys.OperationFailed);

        return ResultT<Unit>.Success(Unit.Value);
    }
}
