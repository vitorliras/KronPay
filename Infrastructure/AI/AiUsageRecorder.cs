using Application.Abstractions.AI;
using Domain.Entities.AI;
using Domain.Interfaces;
using Domain.Interfaces.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.AI;

public sealed class AiUsageRecorder : IAiUsageRecorder
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AiUsageRecorder> _logger;

    public AiUsageRecorder(
        IServiceScopeFactory scopeFactory,
        ILogger<AiUsageRecorder> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task RecordAsync(AiUsageLog log)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();

            var repository = scope.ServiceProvider.GetRequiredService<IAiUsageLogRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var added = await repository.AddAsync(log);
            if (!added || !await unitOfWork.CommitAsync())
                _logger.LogWarning("AI usage log was not persisted.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to record AI usage.");
        }
    }
}
