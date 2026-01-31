namespace Application.DTOs.Users;

public sealed record UserResponse(
    int Id,
    string Name,
    string UserName,
    string Email
);
