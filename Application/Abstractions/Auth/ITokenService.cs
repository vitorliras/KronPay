using KronPay.Domain.Entities.Users;

namespace Application.Abstractions.Auth;

public interface ITokenService
{
    TokenResult GenerateToken(User user);
    RefreshTokenResult GenerateRefreshToken();
    string HashRefreshToken(string refreshToken);
}

public sealed record TokenResult(string Token, DateTime ExpiresAt);
