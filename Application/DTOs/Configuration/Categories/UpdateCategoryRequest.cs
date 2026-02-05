
namespace Application.DTOs.Configuration.Category
{
    public sealed record UpdateCategoryRequest(
     int Id,
     int UserId,
     string Description
 );

}
