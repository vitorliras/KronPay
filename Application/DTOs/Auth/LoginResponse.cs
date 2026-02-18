namespace Application.DTOs.Auth;

public sealed class LoginResponse
{
    public string AccessToken { get; init; } = default!;
    public DateTime ExpiresAt { get; init; }
    public string? User { get; init; }
    public string? TypeUser { get; init; } 

}
