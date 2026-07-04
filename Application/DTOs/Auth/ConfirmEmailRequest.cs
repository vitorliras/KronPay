namespace Application.DTOs.Auth;

public sealed record ConfirmEmailRequest(string Email, string Code);
