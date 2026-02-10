
using Castle.Components.DictionaryAdapter.Xml;
using Domain.Entities.Transactions;
using Domain.Interfaces.Transactions;
using Microsoft.EntityFrameworkCore;
using Shared.Results;

namespace Infrastructure.Repositories.Transactions
{
    public sealed class TransactionRepository : ITransactionRepository
    {
        private readonly AppDbContext _context;

        public TransactionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddAsync(Transaction transaction)
        {
            if (transaction is null)
                return false;

            var entry = await _context.Transactions.AddAsync(transaction);

            return entry.State == EntityState.Added;
        }

        public async Task<bool> AddRangeAsync(IEnumerable<Transaction> transactions)
        {
            if (transactions is null || !transactions.Any())
                return false;

            await _context.Transactions.AddRangeAsync(transactions);

            return transactions.All(t =>
                _context.Entry(t).State == EntityState.Added);
        }


        public async Task<Transaction?> GetByIdAsync(int id, int userId)
        {
            return await _context.Transactions
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
        }

        public async Task<IEnumerable<Transaction>> GetByYearAsync(int userId, int year)
        {
            return await _context.Transactions
                .Where(t =>
                    t.UserId == userId &&
                    t.TransactionDate.Year == year)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetByMonthAsync(int userId, int year, int month)
        {
            return await _context.Transactions
                .Where(t =>
                    t.UserId == userId &&
                    t.TransactionDate.Year == year &&
                    t.TransactionDate.Month == month)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetByPeriodAsync(int userId, DateTime startDate, DateTime endDate)
        {
            return await _context.Transactions
                .Where(t =>
                    t.UserId == userId &&
                    t.TransactionDate >= startDate &&
                    t.TransactionDate <= endDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetByGroupAsync(int transactionGroupId, int userId)
        {
            return await _context.Transactions
                .Where(t =>
                    t.TransactionGroupId == transactionGroupId &&
                    t.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetFutureByGroupAsync(int transactionGroupId, int userId, DateTime fromDate)
        {
            return await _context.Transactions
                .Where(t =>
                    t.TransactionGroupId == transactionGroupId &&
                    t.UserId == userId &&
                    t.TransactionDate >= fromDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetAllTransactionAsync(int userId)
        {
            return await _context.Transactions
                .Where(t => t.UserId == userId)
                .ToListAsync();
        }

        public Task<bool> UpdateAsync(Transaction transaction)
        {
            var result = _context.Transactions.Update(transaction);
            return Task.FromResult(result.State == EntityState.Modified);
        }

        public Task<bool> UpdateRangeAsync(IEnumerable<Transaction> transactions)
        {
            if (transactions is null || !transactions.Any())
                return Task.FromResult(false);

            _context.Transactions.UpdateRange(transactions);

            return Task.FromResult(
                transactions.Any(t =>
                    _context.Entry(t).State == EntityState.Modified)
            );
        }

        public async Task<bool> UpdatePendingByGroupAsync(int transactionGroupId, int userId, Action<Transaction> updateAction)
        {
            var transactions = await _context.Transactions
                .Where(t =>
                    t.TransactionGroupId == transactionGroupId &&
                    t.UserId == userId &&
                    t.Status == "P")
                .ToListAsync();

            if (!transactions.Any())
                return false;

            foreach (var transaction in transactions)
            {
                updateAction(transaction);
            }

            return transactions.Any(t => _context.Entry(t).State == EntityState.Modified);
        }

        public Task<bool> DeleteAsync(Transaction transaction)
        {
            var result = _context.Transactions.Remove(transaction);
            return Task.FromResult(result.State == EntityState.Deleted);
        }

        public async Task<bool> DeleteFutureByGroupAsync(int transactionGroupId, int userId, DateTime fromDate)
        {
            var transactions = await _context.Transactions
                .Where(t =>
                    t.TransactionGroupId == transactionGroupId &&
                    t.UserId == userId &&
                    t.TransactionDate >= fromDate)
                .ToListAsync();

            if (!transactions.Any())
                return false;

            _context.Transactions.RemoveRange(transactions);

            return transactions.All(t => _context.Entry(t).State == EntityState.Deleted);
        }

        public async Task<bool> DeleteByGroupAsync(int transactionGroupId, int userId)
        {
            var transactions = await _context.Transactions
                .Where(t =>
                    t.TransactionGroupId == transactionGroupId &&
                    t.UserId == userId)
                .ToListAsync();

            if (!transactions.Any())
                return false;

            _context.Transactions.RemoveRange(transactions);

            return transactions.All(t => _context.Entry(t).State == EntityState.Deleted);
        }

      
    }


}

