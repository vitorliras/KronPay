using KronPay.Domain.Entities.Users;

namespace Domain.Interfaces.Users;

public interface IUserProfilePhotoRepository
{
    Task<UserProfilePhoto?> GetByUserIdAsync(int userId);
    Task<bool> ExistsAsync(int userId);
    Task<bool> AddAsync(UserProfilePhoto photo);
    bool Update(UserProfilePhoto photo);
    Task<bool> DeleteAsync(UserProfilePhoto photo);
}
