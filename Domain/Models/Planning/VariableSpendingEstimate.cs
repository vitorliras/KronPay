using Domain.Enums.Planning;

namespace Domain.Models.Planning;

public sealed record VariableSpendingEstimate(
    decimal MonthlyAmount,
    ConfidenceLevel Confidence);
