using KronPay.Domain.Entities.Users;
namespace Domain.interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<TypeUser?> GetTypeUserByCode(string code);
    Task<User?> GetByCpfAsync(string cpf);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByIdAsync(int id);
    Task<bool> AddAsync(User user);
    bool Update(User user);

    // Ampliação da SPEC 0024 (motor de notificações): não existe "usuário atual" dentro do
    // job agendado, então ele precisa iterar todos os usuários explicitamente.
    Task<IReadOnlyList<int>> GetAllUserIdsAsync();
}
