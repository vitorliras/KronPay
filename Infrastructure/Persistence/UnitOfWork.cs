using Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private readonly ILogger<UnitOfWork> _logger;

    public UnitOfWork(AppDbContext context, ILogger<UnitOfWork> logger)
    {
        _context = context;
        _logger = logger;
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
            _logger.LogError(e, "Falha ao persistir alterações no banco de dados");
            return false;
        }
    }
}
