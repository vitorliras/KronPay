
using Domain.Entities.banks;

namespace Doamain.Interface.Banks
{
    public interface IBankRepository
    {
        Task AddAsync(BankConnection connection);
        Task<BankConnection?> GetByExternalConnectionIdAsync(string externalConnectionId);
        Task<IEnumerable<Bank>> GetAllAsync();

    }
}
