using Application.DTOs.Configuration.CategoryItems;

namespace Application.DTOs.Configuration;

public sealed record DeactivateCategoryItemSelectRequest(
    IReadOnlyCollection<DeactivateCategoryItemRequest> CategoryItems
);
