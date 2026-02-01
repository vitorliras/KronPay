namespace Application.DTOs.Configuration.CategoryItems;

public sealed record CategoryItemResponse(
    int Id,
    string Description,
    int CategoryId,
    bool Active
);