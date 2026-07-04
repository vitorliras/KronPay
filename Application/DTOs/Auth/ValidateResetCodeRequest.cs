namespace Application.DTOs.Auth;

public sealed record ValidateResetCodeRequest(string Email, string Code);
