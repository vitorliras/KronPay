using System.Security.Cryptography;
using System.Text;

namespace Domain.Services.Auth;

public sealed class VerificationCodeService : IVerificationCodeService
{
    private const int CodeLength = 6;
    private static readonly TimeSpan Validity = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan ResendCooldown = TimeSpan.FromSeconds(30);

    public string GenerateCode() =>
        RandomNumberGenerator.GetInt32(0, 1_000_000).ToString($"D{CodeLength}");

    public string Hash(string code) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(code)));

    public bool Matches(string code, string hash) => Hash(code) == hash;

    public DateTime ComputeExpiresAt(DateTime now) => now.Add(Validity);

    public bool IsResendCooldownActive(DateTime lastCodeCreatedAt, DateTime now) =>
        now < lastCodeCreatedAt.Add(ResendCooldown);
}
