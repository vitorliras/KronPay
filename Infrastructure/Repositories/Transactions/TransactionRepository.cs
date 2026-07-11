
using Castle.Components.DictionaryAdapter.Xml;
using Domain.Entities.Transactions;
using Domain.Interfaces.Transactions;
using KronPay.Domain.Entities.Users;
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
                .Include(t => t.TransactionGroup)
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
        }

        public async Task<IEnumerable<Transaction>> GetByYearAsync(int userId, int year)
        {
            return await _context.Transactions
                .Include(t => t.TransactionGroup)
                .Where(t =>
                    t.UserId == userId &&
                    t.TransactionDate.Year == year)
                .OrderBy(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetByMonthAsync(int userId, int year, int month)
        {
            return await _context.Transactions
                .Include(t => t.TransactionGroup)
                .Where(t =>
                    t.UserId == userId &&
                    t.TransactionDate.Year == year &&
                    t.TransactionDate.Month == month)
                .OrderBy(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetByDatesAsync(int userId, DateTime startDate, DateTime? endDate)
        {
            return await _context.Transactions
                .Include(t => t.TransactionGroup)
                .Where(t =>
                    t.UserId == userId &&
                    t.TransactionDate >= startDate &&
                    (!endDate.HasValue || t.TransactionDate <= endDate.Value))
                .OrderBy(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetByPeriodAsync(int userId, DateTime startDate, DateTime endDate)
        {
            return await _context.Transactions
                .Include(t => t.TransactionGroup)
                .Where(t =>
                    t.UserId == userId &&
                    t.TransactionDate >= startDate &&
                    t.TransactionDate <= endDate)
                .OrderBy(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetByGroupAsync(int transactionGroupId, int userId)
        {
            return await _context.Transactions
                .Include(t => t.TransactionGroup)
                .Where(t =>
                    t.TransactionGroupId == transactionGroupId &&
                    t.UserId == userId)
                .OrderBy(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetFutureByGroupAsync(int transactionGroupId, int userId, DateTime fromDate)
        {
            return await _context.Transactions
                .Include(t => t.TransactionGroup)
                .Where(t =>
                    t.TransactionGroupId == transactionGroupId &&
                    t.UserId == userId &&
                    t.TransactionDate >= fromDate)
                .OrderBy(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetAllTransactionAsync(int userId)
        {
            return await _context.Transactions
                .Include(t => t.TransactionGroup)
                .Where(t => t.UserId == userId)
                .OrderBy(t => t.TransactionDate)
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
                    (t.Status == "O" || t.Status == "E"))
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

        public Task<bool> DeleteRangeAsync(IEnumerable<Transaction> transactions)
        {
            _context.Transactions.RemoveRange(transactions);

            var allMarkedAsDeleted = transactions.All(t =>
                _context.Entry(t).State == EntityState.Deleted);

            return Task.FromResult(allMarkedAsDeleted);
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

        public async Task<IEnumerable<Transaction>> GetOverdueOrDueSoonAsync(int userId, DateTime today)
        {
            var tomorrow = today.AddDays(1);

            return await _context.Transactions
                .AsNoTracking()
                .Where(t =>
                    t.UserId == userId &&
                    t.CodTypeTransaction == "E" &&
                    (t.Status == "O" || t.Status == "E") &&
                    t.TransactionDate.Date <= tomorrow)
                .OrderBy(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<DateTime?> GetLastTransactionDateAsync(int userId)
        {
            return await _context.Transactions
                .AsNoTracking()
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => (DateTime?)t.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> ExistsByPaymentMethodIdAsync(int paymentMethodId)
        {
            return await _context.Transactions
                .AsNoTracking()
                .AnyAsync(t => t.IdPaymentMethod == paymentMethodId);
        }

        public async Task<bool> ExistsByCategoryIdAsync(int categoryId)
        {
            return await _context.Transactions
                .AsNoTracking()
                .AnyAsync(t => t.CategoryId == categoryId);
        }

        public async Task<bool> ExistsByCategoryItemIdAsync(int categoryItemId)
        {
            return await _context.Transactions
                .AsNoTracking()
                .AnyAsync(t => t.CategoryItemId == categoryItemId);
        }
    }


}

