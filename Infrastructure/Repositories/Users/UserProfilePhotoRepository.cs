using Domain.Interfaces.Users;
using Infrastructure.Context;
using KronPay.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Users;

public sealed class UserProfilePhotoRepository : IUserProfilePhotoRepository
{
    private readonly AppDbContext _context;

    public UserProfilePhotoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UserProfilePhoto?> GetByUserIdAsync(int userId)
    {
        return await _context.UserProfilePhotos
            .FirstOrDefaultAsync(x => x.UserId == userId);
    }

    public async Task<bool> ExistsAsync(int userId)
    {
        return await _context.UserProfilePhotos
            .AsNoTracking()
            .AnyAsync(x => x.UserId == userId);
    }

    public async Task<bool> AddAsync(UserProfilePhoto photo)
    {
        var result = await _context.UserProfilePhotos.AddAsync(photo);
        return result.State == EntityState.Added;
    }

    public bool Update(UserProfilePhoto photo)
    {
        var result = _context.UserProfilePhotos.Update(photo);
        return result.State == EntityState.Modified;
    }

    public Task<bool> DeleteAsync(UserProfilePhoto photo)
    {
        var result = _context.UserProfilePhotos.Remove(photo);
        return Task.FromResult(result.State == EntityState.Deleted);
    }
}
