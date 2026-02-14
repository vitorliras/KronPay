using Domain.Interfaces;

namespace Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CommitAsync(CancellationToken? cancellationToken = null)
    {
        try
        {
            if (cancellationToken == null)
                return await _context.SaveChangesAsync() > 0;
            else
                return await _context.SaveChangesAsync((CancellationToken)cancellationToken) > 0;

        }
        catch (Exception e)
        {
            return false;
        }
    }
}
