using Domain.Exceptions;
using Shared.Localization;

namespace Domain.ValueObjects.Planning;

/// <summary>
/// Value Object que descreve a repetição de um compromisso previsto e sabe enumerar
/// suas ocorrências dentro de um intervalo. Periodicidade em code string:
/// "U" = único/pontual, "M" = mensal, "S" = semanal, "A" = anual.
/// </summary>
public sealed class Recurrence
{
    public string Periodicity { get; }
    public DateTime StartDate { get; }
    public DateTime? EndDate { get; }

    public Recurrence(string periodicity, DateTime startDate, DateTime? endDate = null)
    {
        if (periodicity is not ("U" or "M" or "S" or "A"))
            throw new DomainException(MessageKeys.InvalidPeriodicity);

        if (endDate.HasValue && endDate.Value.Date < startDate.Date)
            throw new DomainException(MessageKeys.InvalidCommitmentPeriod);

        Periodicity = periodicity;
        StartDate = startDate.Date;
        EndDate = endDate?.Date;
    }

    /// <summary>
    /// Datas de ocorrência (apenas a parte de data) dentro de [from, to], respeitando
    /// a data final da recorrência. Ancorado no dia da <see cref="StartDate"/>.
    /// </summary>
    public IEnumerable<DateTime> OccurrencesBetween(DateTime from, DateTime to)
    {
        var windowStart = from.Date;
        var windowEnd = to.Date;

        if (EndDate.HasValue && EndDate.Value < windowEnd)
            windowEnd = EndDate.Value;

        if (windowEnd < windowStart)
            yield break;

        if (Periodicity == "U")
        {
            if (StartDate >= windowStart && StartDate <= windowEnd)
                yield return StartDate;
            yield break;
        }

        for (var n = 0; ; n++)
        {
            var occurrence = Advance(n);

            if (occurrence > windowEnd)
                yield break;

            if (occurrence >= windowStart)
                yield return occurrence;
        }
    }

    private DateTime Advance(int n) => Periodicity switch
    {
        "M" => StartDate.AddMonths(n),
        "S" => StartDate.AddDays(7 * n),
        "A" => StartDate.AddYears(n),
        _ => StartDate
    };
}
