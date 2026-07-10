using Application.Services.Gamification;
using Domain.Entities.Gamification;
using Domain.Interfaces;
using Domain.Interfaces.Gamification;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Workers;

public sealed class GamificationEvaluationWorker : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromMinutes(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<GamificationEvaluationWorker> _logger;

    public GamificationEvaluationWorker(IServiceScopeFactory scopeFactory, ILogger<GamificationEvaluationWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(PollInterval);

        do
        {
            await RunIfDueAsync(stoppingToken);
        }
        while (!stoppingToken.IsCancellationRequested
            && await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task RunIfDueAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var runRepository = scope.ServiceProvider.GetRequiredService<IGamificationEvaluationRunRepository>();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var run = await runRepository.GetAsync();
            var isNew = run is null;
            run ??= new GamificationEvaluationRun();

            var today = DateTime.UtcNow.Date;
            if (!isNew && run.RanAt.Date >= today)
                return;

            var orchestrator = scope.ServiceProvider.GetRequiredService<IGamificationEvaluationOrchestrator>();
            var summary = await orchestrator.RunAsync();

            run.MarkRun(DateTime.UtcNow, summary.UsersProcessed, summary.EventsTriggered, summary.BadgesUnlocked);

            var persisted = isNew ? await runRepository.AddAsync(run) : runRepository.Update(run);
            if (!persisted || !await uow.CommitAsync())
                _logger.LogError("Falha ao registrar a última execução da avaliação de gamificação.");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Falha inesperada ao avaliar gamificação no GamificationEvaluationWorker.");
        }
    }
}
