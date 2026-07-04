namespace Application.DTOs.Auth;

public sealed class LoginRequest
{
    public string Email { get; init; } = default!;
    public string Password { get; init; } = default!;
    public bool RememberMe { get; init; }
}
