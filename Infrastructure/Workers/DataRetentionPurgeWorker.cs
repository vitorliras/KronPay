using Domain.Entities.DataRetention;
using Domain.Interfaces;
using Domain.Interfaces.DataRetention;
using Application.DataRetention;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Workers;

public sealed class DataRetentionPurgeWorker : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromMinutes(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DataRetentionPurgeWorker> _logger;

    public DataRetentionPurgeWorker(IServiceScopeFactory scopeFactory, ILogger<DataRetentionPurgeWorker> logger)
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
            var runRepository = scope.ServiceProvider.GetRequiredService<IDataRetentionPurgeRunRepository>();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var run = await runRepository.GetAsync();
            var isNew = run is null;
            run ??= new DataRetentionPurgeRun();

            var today = DateTime.UtcNow.Date;
            if (!isNew && run.LastRunAt.Date >= today)
                return;

            var orchestrator = scope.ServiceProvider.GetRequiredService<IDataRetentionPurgeOrchestrator>();
            await orchestrator.RunAsync();

            run.MarkRun(DateTime.UtcNow);

            var persisted = isNew ? await runRepository.AddAsync(run) : runRepository.Update(run);
            if (!persisted || !await uow.CommitAsync())
                _logger.LogError("Falha ao registrar a ultima execucao da purga de retencao de dados.");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Falha inesperada ao executar o DataRetentionPurgeWorker.");
        }
    }
}
