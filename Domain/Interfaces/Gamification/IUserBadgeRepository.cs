using Domain.Entities.Gamification;
using Domain.Enums.Gamification;

namespace Domain.Interfaces.Gamification;

public interface IUserBadgeRepository
{
    Task<bool> AddAsync(UserBadge badge);
    Task<IEnumerable<UserBadge>> GetByUserIdAsync(int userId);
    Task<bool> ExistsAsync(int userId, BadgeCode code);
}
