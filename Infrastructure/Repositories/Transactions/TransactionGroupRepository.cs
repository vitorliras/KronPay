
using Castle.Components.DictionaryAdapter.Xml;
using Domain.Entities.Transactions;
using Domain.Interfaces.Transactions;
using Microsoft.EntityFrameworkCore;
using Shared.Results;

namespace Infrastructure.Repositories.Transactions
{
    public sealed class TransactionGroupRepository : ITransactionGroupRepository
    {
        private readonly AppDbContext _context;

        public TransactionGroupRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddAsync(TransactionGroup group)
        {
            if (group is null)
                return false;

            var entry = await _context.TransactionsGroups.AddAsync(group);

            return entry.State == EntityState.Added;
        }

        public async Task<IEnumerable<TransactionGroup>> GetAllAsync(int userId)
        {
            return await _context.TransactionsGroups
                .Where(g => g.UserId == userId)
                .OrderByDescending(g => g.CreatedAt)
                .ToListAsync();
        }

        public async Task<TransactionGroup?> GetByIdAsync(int id, int userId)
        {
            return await _context.TransactionsGroups
                .FirstOrDefaultAsync(g =>
                    g.Id == id &&
                    g.UserId == userId);
        }

        public  Task<bool> UpdateAsync(TransactionGroup group)
        {
            var result = _context.TransactionsGroups.Update(group);
            return Task.FromResult(result.State == EntityState.Modified);
        }

        public Task<bool> DeleteAsync(TransactionGroup group)
        {
            _context.TransactionsGroups.Remove(group);
            return Task.FromResult(true);
        }

    }

}

