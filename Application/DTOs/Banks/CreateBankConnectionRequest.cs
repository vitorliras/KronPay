
namespace Application.DTOs.Banks
{
    public sealed record CreateBankConnectionRequest(
        string ExternalConnectionId,
        string InstitutionCode,
        string InstitutionName
    );

}
