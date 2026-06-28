
namespace Application.DTOs.Banks
{
    public sealed record BankConnectionResponse(
         int Id,
         string InstitutionName,
         bool Active
     );

}
