namespace Application.DTOs.Configuration.CategoryItems;

public sealed record UpdateCategoryItemRequest(
    int Id,
    int CategoryId,
    string Description
);
