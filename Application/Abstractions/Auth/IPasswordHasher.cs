
using KronPay.Domain.Entities.Users;
namespace Application.Abstractions.Auth;

public interface ITokenService
{
    string GenerateToken(User user);
    DateTime GetExpiration();
}
