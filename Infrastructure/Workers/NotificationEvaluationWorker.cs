using Domain.Entities.Notifications;
using Domain.Interfaces;
using Domain.Interfaces.Notifications;
using Application.Notifications;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Workers;

public sealed class NotificationEvaluationWorker : BackgroundService
{
    // Intervalo curto de propósito: o catch-up (ADR 0020) só faz uma leitura barata
    // ("o dia já virou desde a última execução?") a cada tick — reavaliar de verdade só
    // acontece quando essa checagem é verdadeira. Um valor baixo aqui não pesa em produção
    // e evita o usuário/desenvolvedor esperar até 30 minutos para ver o efeito de uma
    // reavaliação forçada manualmente (ex.: apagar a linha de controle para testar).
    private static readonly TimeSpan PollInterval = TimeSpan.FromMinutes(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NotificationEvaluationWorker> _logger;

    public NotificationEvaluationWorker(IServiceScopeFactory scopeFactory, ILogger<NotificationEvaluationWorker> logger)
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

    // Catch-up (ADR 0020): roda imediatamente ao subir a Api e a cada tick se o dia (UTC)
    // ainda não tiver sido avaliado — não depende de nenhum horário fixo, então funciona
    // mesmo que a Api fique dias desligada e volte a rodar num horário qualquer.
    private async Task RunIfDueAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var runRepository = scope.ServiceProvider.GetRequiredService<INotificationEvaluationRunRepository>();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var run = await runRepository.GetAsync();
            var isNew = run is null;
            run ??= new NotificationEvaluationRun();

            var today = DateTime.UtcNow.Date;
            if (!isNew && run.LastRunAt.Date >= today)
                return;

            var orchestrator = scope.ServiceProvider.GetRequiredService<INotificationEvaluationOrchestrator>();
            await orchestrator.RunAsync();

            run.MarkRun(DateTime.UtcNow);

            var persisted = isNew ? await runRepository.AddAsync(run) : runRepository.Update(run);
            if (!persisted || !await uow.CommitAsync())
                _logger.LogError("Falha ao registrar a última execução da avaliação de notificações.");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Falha inesperada ao avaliar notificações no NotificationEvaluationWorker.");
        }
    }
}
