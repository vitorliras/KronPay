using Domain.Entities.Auth;
using Domain.Enums.Auth;
using Domain.Interfaces.Auth;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Auth;

public sealed class VerificationCodeRepository : IVerificationCodeRepository
{
    private readonly AppDbContext _context;

    public VerificationCodeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> AddAsync(VerificationCode verificationCode)
    {
        var result = await _context.VerificationCodes.AddAsync(verificationCode);
        return result.State == EntityState.Added;
    }

    public bool Update(VerificationCode verificationCode)
    {
        var result = _context.VerificationCodes.Update(verificationCode);
        return result.State == EntityState.Modified;
    }

    public async Task<VerificationCode?> GetLastAsync(int userId, VerificationPurpose purpose)
    {
        return await _context.VerificationCodes
            .Where(x => x.UserId == userId && x.Purpose == purpose)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();
    }
}
