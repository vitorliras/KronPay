using Domain.Entities.Auth;
using Domain.Interfaces.Auth;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Auth;

public sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _context;

    public RefreshTokenRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> AddAsync(RefreshToken refreshToken)
    {
        var result = await _context.RefreshTokens.AddAsync(refreshToken);
        return result.State == EntityState.Added;
    }

    public bool Update(RefreshToken refreshToken)
    {
        var result = _context.RefreshTokens.Update(refreshToken);
        return result.State == EntityState.Modified;
    }

    public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash)
    {
        return await _context.RefreshTokens
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash);
    }
}
