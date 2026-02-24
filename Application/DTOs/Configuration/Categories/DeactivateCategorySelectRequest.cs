using Application.DTOs.Configuration.Categories;
using Application.DTOs.Configuration.CategoryItems;
using Domain.Entities.Transactions;

namespace Application.DTOs.Configuration;

public sealed record DeactivateCategorySelectRequest(
    IReadOnlyCollection<DeactivateCategoryRequest> Categories
);
