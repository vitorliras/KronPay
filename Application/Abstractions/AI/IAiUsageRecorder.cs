using Domain.Entities.AI;

namespace Application.Abstractions.AI;

public interface IAiUsageRecorder
{
    Task RecordAsync(AiUsageLog log);
}
