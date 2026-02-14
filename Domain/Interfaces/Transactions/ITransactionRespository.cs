using Domain.Entities.Transactions;

namespace Domain.Interfaces.Transactions
{
    public interface ITransactionRepository
    {
        Task<bool> AddAsync(Transaction transaction);
        Task<bool> AddRangeAsync(IEnumerable<Transaction> transactions);
        Task<Transaction?> GetByIdAsync(int id, int userId);
        Task<IEnumerable<Transaction>> GetByMonthAsync(int userId, int year, int month);
        Task<IEnumerable<Transaction>> GetByYearAsync(int userId, int year);
        Task<IEnumerable<Transaction>> GetByPeriodAsync(int userId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<Transaction>> GetByGroupAsync(int transactionGroupId, int userId);
        Task<IEnumerable<Transaction>> GetFutureByGroupAsync(int transactionGroupId, int userId, DateTime fromDate);
        Task<IEnumerable<Transaction>> GetAllTransactionAsync(int userId);
        Task<bool> UpdateAsync(Transaction transaction);
        Task<bool> UpdateRangeAsync(IEnumerable<Transaction> transactions);
        Task<bool> UpdatePendingByGroupAsync(int transactionGroupId,int userId, Action<Transaction> updateAction);
        Task<bool> DeleteAsync(Transaction transaction);
        Task<bool> DeleteFutureByGroupAsync(int transactionGroupId, int userId, DateTime fromDate);
        Task<bool> DeleteByGroupAsync(int transactionGroupId, int userId);
        Task<bool> DeleteRangeAsync(IEnumerable<Transaction> transactions);

    }


}
