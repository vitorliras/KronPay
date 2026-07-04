namespace Application.DTOs.Auth;

public sealed record RefreshTokenResponse(string AccessToken, DateTime ExpiresAt, string RefreshToken);
