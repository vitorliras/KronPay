using Doamain.Interface.Banks;
using Domain.Entities.banks;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Banks
{
    public class BankRepository : IBankRepository
    {
        private readonly AppDbContext _context;

        public BankRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(BankConnection connection)
        {
            await _context.BankConnections.AddAsync(connection);
        }

        public async Task<IEnumerable<Bank>> GetAllAsync()
        {
            return await _context.Banks
                .Where(x => x.Active).ToListAsync();
        }

        public async Task<BankConnection?> GetByExternalConnectionIdAsync(string externalConnectionId)
        {
            return await _context.BankConnections
                .FirstOrDefaultAsync(x =>
                    x.ExternalConnectionId == externalConnectionId);
        }

       
    }
}
