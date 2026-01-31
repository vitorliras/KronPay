namespace Application.DTOs.Auth;

public sealed class LoginResponse
{
    public string AccessToken { get; init; } = default!;
    public DateTime ExpiresAt { get; init; }
}
