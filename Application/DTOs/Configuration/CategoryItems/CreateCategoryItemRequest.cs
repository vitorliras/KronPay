
namespace Application.DTOs.Configuration.CategoryItems
{
    public sealed record CreateCategoryItemRequest(
    int CategoryId,
    string Description
    );
}
