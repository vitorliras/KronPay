namespace Application.DTOs.Configuration.Categories;

public sealed record CategoryResponse(
    int Id,
    string Description,
    string CodTypeTransaction,
    bool Active
);