namespace Application.DTOs.Auth;

public sealed record ResetPasswordRequest(string Email, string Code, string NewPassword, string ConfirmPassword);
