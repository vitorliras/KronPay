namespace Domain.Services.Auth;

public interface IVerificationCodeService
{
    string GenerateCode();
    string Hash(string code);
    bool Matches(string code, string hash);
    DateTime ComputeExpiresAt(DateTime now);
    bool IsResendCooldownActive(DateTime lastCodeCreatedAt, DateTime now);
}
