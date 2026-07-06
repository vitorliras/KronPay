namespace Application.DTOs.Configuration.Categories;

public sealed record CreateCategoryRequest(
    string Description,
    string CodTypeTransaction,
    bool IsCardInvoiceCategory = false
);
