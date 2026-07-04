namespace Application.Abstractions.Auth;

public sealed record RefreshTokenResult(string Token, DateTime ExpiresAt);
