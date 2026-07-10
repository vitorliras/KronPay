namespace Application.Services.Gamification;

public sealed record GamificationEvaluationSummary(int UsersProcessed, int EventsTriggered, int BadgesUnlocked);

public interface IGamificationEvaluationOrchestrator
{
    Task<GamificationEvaluationSummary> RunAsync();
}
