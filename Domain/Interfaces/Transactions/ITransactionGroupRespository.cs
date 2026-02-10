using Domain.Entities.Configuration;
using Domain.Entities.Transactions;

namespace Domain.Interfaces.Transactions
{
    public interface ITransactionGroupRepository
    {
        Task<bool> AddAsync(TransactionGroup group);

        Task<TransactionGroup?> GetByIdAsync(int id, int userId);

        Task<IEnumerable<TransactionGroup>> GetAllAsync(int userId);

        Task<bool> UpdateAsync(TransactionGroup group);

        Task<bool> DeleteAsync(TransactionGroup group);
    }



}
