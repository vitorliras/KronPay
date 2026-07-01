namespace Domain.Enums.Planning;

/// <summary>Origem de um fluxo financeiro (para rastreabilidade da projeção).</summary>
public enum FlowOrigin
{
    Transaction = 1,
    CardInvoice = 2,
    PlannedCommitment = 3,
    VariableEstimate = 4,
    Simulation = 5
}
