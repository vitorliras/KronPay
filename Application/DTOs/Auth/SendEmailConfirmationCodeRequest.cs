namespace Application.DTOs.Auth;

public sealed record SendEmailConfirmationCodeRequest(int UserId, string Email);
