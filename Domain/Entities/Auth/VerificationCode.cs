using Domain.Enums.Auth;
using Domain.Exceptions;
using Shared.Localization;

namespace Domain.Entities.Auth;

public sealed class VerificationCode
{
    private const int MaxAttempts = 5;

    public int Id { get; private set; }
    public int UserId { get; private set; }
    public string CodeHash { get; private set; }
    public VerificationPurpose Purpose { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool Used { get; private set; }
    public int AttemptsCount { get; private set; }

    protected VerificationCode() { }

    public VerificationCode(
        int userId,
        string codeHash,
        VerificationPurpose purpose,
        DateTime expiresAt)
    {
        if (userId <= 0)
            throw new DomainException(MessageKeys.InvaldUser);

        if (string.IsNullOrWhiteSpace(codeHash))
            throw new DomainException(MessageKeys.InvalidVerificationCode);

        UserId = userId;
        CodeHash = codeHash;
        Purpose = purpose;
        ExpiresAt = expiresAt;
        CreatedAt = DateTime.UtcNow;
        Used = false;
        AttemptsCount = 0;
    }

    public bool IsExpired(DateTime now) => now >= ExpiresAt;

    public bool ExceededMaxAttempts() => AttemptsCount >= MaxAttempts;

    public void RegisterFailedAttempt() => AttemptsCount++;

    public void MarkAsUsed() => Used = true;

    public void Invalidate() => Used = true;
}
