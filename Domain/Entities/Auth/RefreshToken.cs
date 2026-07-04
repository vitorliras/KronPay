using Domain.Exceptions;
using Shared.Localization;

namespace Domain.Entities.Auth;

public sealed class RefreshToken
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public string TokenHash { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    protected RefreshToken() { }

    public RefreshToken(int userId, string tokenHash, DateTime expiresAt)
    {
        if (userId <= 0)
            throw new DomainException(MessageKeys.InvaldUser);

        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new DomainException(MessageKeys.InvalidRefreshToken);

        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
        CreatedAt = DateTime.UtcNow;
    }

    public bool IsRevoked => RevokedAt is not null;

    public bool IsExpired(DateTime now) => now >= ExpiresAt;

    public void Revoke() => RevokedAt = DateTime.UtcNow;
}
