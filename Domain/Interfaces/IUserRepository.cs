using KronPay.Domain.Entities.Users;
namespace Domain.interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<TypeUser?> GetTypeUserByCode(string code);
    Task<User?> GetByCpfAsync(string cpf);
    Task<User?> GetByUsernameAsync(string username);
    Task<bool> AddAsync(User user);
    bool Update(User user);
}
