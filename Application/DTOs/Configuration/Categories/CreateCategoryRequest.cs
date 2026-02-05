namespace Application.DTOs.Configuration.Categories;

public sealed record CreateCategoryRequest(
    int UserId,
    string Description,
    string CodTypeTransaction
);
