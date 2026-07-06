using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Configuration.Categories;
using Application.DTOs.Configuration.Category;
using Domain.Entities.Configuration;
using Domain.Interfaces;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Categories;

public sealed class CreateCategoryUseCase
    : IUseCase<CreateCategoryRequest, CategoryResponse>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreateCategoryUseCase(ICategoryRepository categoryrepository, IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _categoryRepository = categoryrepository;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<CategoryResponse>> ExecuteAsync(CreateCategoryRequest request)
    {
        var userId = _currentUser.UserId;

        var category = await _categoryRepository.GetByDescriptionAsync(request.Description, userId);

        if (category is not null)
            return ResultEntity<CategoryResponse>.Failure(MessageKeys.DescriptionAlreadyExists);

         category = new Category(
            userId,
            request.Description,
            request.CodTypeTransaction
        );

        if (request.IsCardInvoiceCategory)
        {
            var previous = await _categoryRepository.GetCardInvoiceCategoryAsync(userId);
            if (previous is not null)
            {
                previous.UnmarkAsCardInvoiceCategory();
                _categoryRepository.Update(previous);
            }

            category.MarkAsCardInvoiceCategory();
        }

        var result = await _categoryRepository.AddAsync(category);
        if (!result)
            return ResultEntity<CategoryResponse>.Failure(MessageKeys.InsertFalied);

        var uow = await _uow.CommitAsync();
        if (!uow)
            return ResultEntity<CategoryResponse>.Failure(MessageKeys.DataPersistenceFailed);

        return ResultEntity<CategoryResponse>.Success(
            new CategoryResponse(
                category.Id,
                category.Description,
                category.CodTypeTransaction,
                category.Active,
                category.IsCardInvoiceCategory
            )
           , MessageKeys.OperationSuccess
        );
    }
}
