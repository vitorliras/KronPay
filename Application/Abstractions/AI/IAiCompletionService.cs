namespace Application.Abstractions.AI;

public interface IAiCompletionService
{
    Task<AiCompletionResult> CompleteAsync(
        AiCompletionRequest request,
        CancellationToken cancellationToken = default);
}
