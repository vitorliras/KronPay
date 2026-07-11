using Application.Configuration.DataRetention;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.DataRetention;

public sealed class DataRetentionPurgeOrchestrator : IDataRetentionPurgeOrchestrator
{
    private readonly IEnumerable<IRetentionPurgeTarget> _targets;
    private readonly DataRetentionSettings _settings;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<DataRetentionPurgeOrchestrator> _logger;

    public DataRetentionPurgeOrchestrator(
        IEnumerable<IRetentionPurgeTarget> targets,
        IOptions<DataRetentionSettings> settings,
        IUnitOfWork uow,
        ILogger<DataRetentionPurgeOrchestrator> logger)
    {
        _targets = targets;
        _settings = settings.Value;
        _uow = uow;
        _logger = logger;
    }

    public async Task<int> RunAsync()
    {
        var cutoff = DateTime.UtcNow.Date.AddDays(-_settings.PurgeAfterDays);
        var totalRemoved = 0;

        foreach (var target in _targets)
            totalRemoved += await target.PurgeAsync(cutoff);

        if (totalRemoved == 0)
            return 0;

        if (!await _uow.CommitAsync())
        {
            _logger.LogError("Falha ao persistir a purga de retencao de dados (corte: {Cutoff}).", cutoff);
            return 0;
        }

        _logger.LogInformation("Purga de retencao de dados removeu {Total} registro(s) (corte: {Cutoff}).", totalRemoved, cutoff);
        return totalRemoved;
    }
}
