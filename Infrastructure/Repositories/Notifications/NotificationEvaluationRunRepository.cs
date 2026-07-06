using Domain.Entities.Notifications;
using Domain.Interfaces.Notifications;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Notifications;

public sealed class NotificationEvaluationRunRepository : INotificationEvaluationRunRepository
{
    private readonly AppDbContext _context;

    public NotificationEvaluationRunRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationEvaluationRun?> GetAsync()
    {
        return await _context.NotificationEvaluationRuns.FirstOrDefaultAsync();
    }

    public async Task<bool> AddAsync(NotificationEvaluationRun run)
    {
        var result = await _context.NotificationEvaluationRuns.AddAsync(run);
        return result.State == EntityState.Added;
    }

    public bool Update(NotificationEvaluationRun run)
    {
        var result = _context.NotificationEvaluationRuns.Update(run);
        return result.State == EntityState.Modified;
    }
}
