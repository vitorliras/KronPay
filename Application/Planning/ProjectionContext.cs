using Domain.Models.Planning;

namespace Application.Planning;

public sealed record ProjectionContext(
    FinancialProjection Projection,
    ProjectionParameters Parameters);
