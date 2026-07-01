using Domain.Enums.Planning;
using Domain.Interfaces.Planning;
using Domain.Models.Planning;
using Domain.ValueObjects.Planning;

namespace Application.Planning.Flows;

/// <summary>
/// Expande os compromissos previstos (recorrentes/pontuais) do usuário em fluxos comprometidos,
/// usando o VO <see cref="Recurrence"/> para enumerar as ocorrências dentro da janela.
/// </summary>
public sealed class CommitmentFlowSource : IFinancialFlowSource
{
    private readonly IPlannedCommitmentRepository _commitments;

    public CommitmentFlowSource(IPlannedCommitmentRepository commitments)
    {
        _commitments = commitments;
    }

    public async Task<IEnumerable<FinancialFlow>> GetFlowsAsync(int userId, DateTime from, DateTime to)
    {
        var commitments = await _commitments.GetByUserAsync(userId);
        var flows = new List<FinancialFlow>();

        foreach (var commitment in commitments)
        {
            var recurrence = new Recurrence(commitment.Periodicity, commitment.StartDate, commitment.EndDate);
            var direction = commitment.Direction == "I" ? FlowDirection.Inflow : FlowDirection.Outflow;

            foreach (var date in recurrence.OccurrencesBetween(from, to))
            {
                flows.Add(new FinancialFlow(
                    date,
                    direction,
                    commitment.Amount,
                    ConfidenceLevel.High,
                    FlowOrigin.PlannedCommitment,
                    commitment.Description));
            }
        }

        return flows;
    }
}
