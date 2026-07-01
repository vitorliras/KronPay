using Domain.Enums.Planning;

namespace Domain.Models.Planning;

public sealed record FinancialFlow(
    DateTime CompetenceDate,
    FlowDirection Direction,
    decimal Amount,
    ConfidenceLevel Confidence,
    FlowOrigin Origin,
    string? Description = null)
{
    public decimal SignedAmount => Direction == FlowDirection.Inflow ? Amount : -Amount;

    public bool IsCommitted => Confidence == ConfidenceLevel.High;
}
