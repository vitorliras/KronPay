using Domain.Enums.Gamification;
using Domain.Exceptions;
using Shared.Localization;

namespace Domain.Entities.Gamification;

public sealed class UserBadge
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public BadgeCode Code { get; private set; }
    public DateTime UnlockedAt { get; private set; }

    protected UserBadge() { }

    public UserBadge(int userId, BadgeCode code)
    {
        if (userId <= 0)
            throw new DomainException(MessageKeys.InvaldUser);

        UserId = userId;
        Code = code;
        UnlockedAt = DateTime.UtcNow;
    }
}
