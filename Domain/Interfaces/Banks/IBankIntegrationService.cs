using Domain.Entities.banks;

namespace Domain.Interface.Bank
{
    public interface IBankIntegrationService
    {
        Task<List<BankConnection>> GetConnectorsAsync();
    }
}
