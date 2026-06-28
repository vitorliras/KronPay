
using Application.DTOs.Banks;

namespace Application.Abstractions.Pluggy
{
    public interface IPluggyService
    {
        Task<string> GetApiKeyAsync();
        Task<ConnectorsResponseDto> GetConnectorsAsync();
        Task<string> GetAccountsAsync(string externalConnectionId);
        Task<ConnectTokenResponse> CreateConnectTokenAsync(string clientUserId);
    }
}
