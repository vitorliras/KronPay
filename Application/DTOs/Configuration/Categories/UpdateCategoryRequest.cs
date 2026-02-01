
namespace Application.DTOs.Configuration.Category
{
    public sealed record CreateCategoryRequest(
    int UserId,
    string Description,
    string CodTypeTransaction
    );
}
