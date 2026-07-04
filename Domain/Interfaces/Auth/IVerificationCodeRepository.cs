using Domain.Entities.Auth;
using Domain.Enums.Auth;

namespace Domain.Interfaces.Auth;

public interface IVerificationCodeRepository
{
    Task<bool> AddAsync(VerificationCode verificationCode);
    bool Update(VerificationCode verificationCode);
    Task<VerificationCode?> GetLastAsync(int userId, VerificationPurpose purpose);
}
