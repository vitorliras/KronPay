namespace Application.Abstractions.AI;

public interface IAiAvailability
{
    ActiveAiProvider? GetActive();
}
