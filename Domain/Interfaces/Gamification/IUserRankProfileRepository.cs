using Domain.Entities.Gamification;

namespace Domain.Interfaces.Gamification;

public interface IUserRankProfileRepository
{
    Task<bool> AddAsync(UserRankProfile profile);
    bool Update(UserRankProfile profile);
    Task<UserRankProfile?> GetByUserIdAsync(int userId);
}
