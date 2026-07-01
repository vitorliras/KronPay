namespace Domain.Enums.Planning;

/// <summary>Veredito de viabilidade de uma decisão financeira.</summary>
public enum ViabilityVerdict
{
    Recommended = 1, // ✅
    Attention = 2,   // ⚠️
    Risk = 3         // ⛔
}
