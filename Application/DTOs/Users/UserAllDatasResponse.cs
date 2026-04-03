using Domain.ValueObjects;
using Domain.ValueObjects.User;

namespace Application.DTOs.Users;

public sealed record UserAllDatasResponse(
    string Name,
    string Username,
    string Phone,
    string Cpf,
    string UserType,
    string Email
);


