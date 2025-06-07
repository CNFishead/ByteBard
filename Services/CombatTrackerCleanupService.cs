using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

public class CombatTrackerCleanupService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<CombatTrackerCleanupService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromHours(6);

    public CombatTrackerCleanupService(IServiceProvider services, ILogger<CombatTrackerCleanupService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await CleanStaleTrackersAsync(stoppingToken);
            try
            {
                await Task.Delay(_interval, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // Swallow if cancellation requested
            }
        }
    }

    private async Task CleanStaleTrackersAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BotDbContext>();
            var threshold = DateTime.UtcNow.AddDays(-7);
            var staleTrackers = await db.CombatTrackers
                .Where(t => t.LastUpdatedAt < threshold)
                .ToListAsync(cancellationToken);

            if (staleTrackers.Count > 0)
            {
                db.CombatTrackers.RemoveRange(staleTrackers);
                await db.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Removed {Count} stale combat trackers", staleTrackers.Count);
            }
            else
            {
                _logger.LogDebug("No stale combat trackers to remove");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while cleaning stale combat trackers");
        }
    }
}
