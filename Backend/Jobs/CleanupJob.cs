namespace Backend.Jobs;

using System;using System.Threading;using System.Threading.Tasks;using Backend.Data;using Backend.Models;using Backend.Services;using Microsoft.EntityFrameworkCore;using Microsoft.Extensions.DependencyInjection;using Microsoft.Extensions.Hosting;using Microsoft.Extensions.Logging;using System.Linq;

public class CleanupJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CleanupJob> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromDays(1);

    public CleanupJob(
        IServiceProvider serviceProvider,
        ILogger<CleanupJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Cleanup job started");

        while (!stoppingToken.IsCancellationRequested)
        {
                        try            {                await CleanupOldActivityData();                await CleanupExpiredVerifications();            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during cleanup");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task CleanupOldActivityData()
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Behold kun de sidste 7 dages aktivitetsdata
        var cutoffDate = DateTime.UtcNow.AddDays(-7).Date;

        var oldActivities = await dbContext.Set<UserDailyActivity>()
            .Where(a => a.Date < cutoffDate)
            .ToListAsync();

        if (oldActivities.Any())
        {
            _logger.LogInformation("Removing {Count} old activity records", oldActivities.Count);
            dbContext.RemoveRange(oldActivities);
                        await dbContext.SaveChangesAsync();        }    }    private async Task CleanupExpiredVerifications()    {        using var scope = _serviceProvider.CreateScope();        var verificationService = scope.ServiceProvider.GetRequiredService<DiscordVerificationService>();                await verificationService.CleanupExpiredVerificationsAsync();    }}