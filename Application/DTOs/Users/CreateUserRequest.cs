namespace Application.DTOs.Users;

public sealed class CreateUserRequest
{
    public string Name { get; init; } = default!;
    public string Username { get; init; } = default!;
    public string Email { get; init; } = default!;
    public string Cpf { get; init; } = default!;
    public string Phone { get; init; } = default!;
    public string Password { get; init; } = default!;
}
