namespace Application.DTOs.Configuration.Categories;

public sealed record UpdateCategoryRequest(
    int Id,
    int UserId,
    string Description
);
