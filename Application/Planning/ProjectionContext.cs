using Domain.Models.Planning;

namespace Application.Planning;

/// <summary>Projeção pronta + os parâmetros usados (compartilhado pelos use cases).</summary>
public sealed record ProjectionContext(
    FinancialProjection Projection,
    ProjectionParameters Parameters);
