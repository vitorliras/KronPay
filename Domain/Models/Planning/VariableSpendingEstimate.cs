using Domain.Enums.Planning;

namespace Domain.Models.Planning;

public sealed record VariableSpendingEstimate(
    decimal CentralDeviation,
    ConfidenceLevel Confidence);
