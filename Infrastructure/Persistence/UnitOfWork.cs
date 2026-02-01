using Domain.Interfaces;

namespace Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CommitAsync()
    {
        try
        {
            return await _context.SaveChangesAsync() > 0;

        }
        catch (Exception e)
        {
            return false;
        }
    }
}
