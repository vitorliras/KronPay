using Domain.Entities.Auth;

namespace Domain.Interfaces.Auth;

public interface IRefreshTokenRepository
{
    Task<bool> AddAsync(RefreshToken refreshToken);
    bool Update(RefreshToken refreshToken);
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash);
    Task<IReadOnlyList<RefreshToken>> GetActiveByUserIdAsync(int userId, DateTime now);
}
