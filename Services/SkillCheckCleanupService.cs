using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

public class SkillCheckCleanupService : BackgroundService
{
  private readonly IServiceProvider _services;
  private readonly ILogger<SkillCheckCleanupService> _logger;
  private readonly TimeSpan _interval = TimeSpan.FromHours(1); // Check every hour

  public SkillCheckCleanupService(IServiceProvider services, ILogger<SkillCheckCleanupService> logger)
  {
    _services = services;
    _logger = logger;
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    while (!stoppingToken.IsCancellationRequested)
    {
      await CleanExpiredSkillChecksAsync(stoppingToken);
      try
      {
        await Task.Delay(_interval, stoppingToken);
      }
      catch (OperationCanceledException)
      {
        // Service is stopping
        break;
      }
    }
  }

  private async Task CleanExpiredSkillChecksAsync(CancellationToken cancellationToken)
  {
    using (var scope = _services.CreateScope())
    {
      var db = scope.ServiceProvider.GetRequiredService<BotDbContext>();

      try
      {
        var handler = new SkillCheckLifecycleHandler(
            scope.ServiceProvider.GetRequiredService<ILogger<SkillCheckLifecycleHandler>>(),
            db);
        await handler.CleanupExpiredSkillChecks();
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error during skill check cleanup");
      }
    }
  }
}
