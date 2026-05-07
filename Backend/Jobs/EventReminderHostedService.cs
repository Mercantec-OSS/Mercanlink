namespace Backend.Jobs;

using Backend.Data;
using Backend.Discord;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

/// <summary>
/// Tjekker hvert 5. minut for events der nærmer sig start (24t og 1t) og sender Discord-reminders.
/// </summary>
public class EventReminderHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventReminderHostedService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);

    public EventReminderHostedService(IServiceProvider serviceProvider, ILogger<EventReminderHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("EventReminderHostedService startet (interval: {Interval}).", _interval);

        // Lille forsinkelse så Discord-bot er klar
        try { await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); } catch (TaskCanceledException) { return; }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessRemindersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved kørsel af event-reminders.");
            }

            try
            {
                await Task.Delay(_interval, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                return;
            }
        }
    }

    private async Task ProcessRemindersAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var announcer = scope.ServiceProvider.GetRequiredService<EventDiscordAnnouncer>();

        var now = DateTime.UtcNow;
        var dayWindowStart = now.AddHours(23);
        var dayWindowEnd = now.AddHours(25);
        var hourWindowStart = now.AddMinutes(45);
        var hourWindowEnd = now.AddMinutes(75);

        var candidates = await dbContext.Events
            .Where(e => e.Status == EventStatus.Published && e.StartsAt > now)
            .Where(e =>
                (!e.ReminderDayBeforeSent && e.StartsAt >= dayWindowStart && e.StartsAt <= dayWindowEnd)
                || (!e.ReminderHourBeforeSent && e.StartsAt >= hourWindowStart && e.StartsAt <= hourWindowEnd))
            .ToListAsync(cancellationToken);

        foreach (var ev in candidates)
        {
            if (cancellationToken.IsCancellationRequested) return;

            try
            {
                if (!ev.ReminderDayBeforeSent && ev.StartsAt >= dayWindowStart && ev.StartsAt <= dayWindowEnd)
                {
                    await announcer.SendReminderAsync(ev, "Reminder – i morgen");
                    ev.ReminderDayBeforeSent = true;
                    ev.UpdatedAt = DateTime.UtcNow;
                }

                if (!ev.ReminderHourBeforeSent && ev.StartsAt >= hourWindowStart && ev.StartsAt <= hourWindowEnd)
                {
                    await announcer.SendReminderAsync(ev, "Det starter snart");
                    ev.ReminderHourBeforeSent = true;
                    ev.UpdatedAt = DateTime.UtcNow;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Kunne ikke sende reminder for event {EventId}", ev.Id);
            }
        }

        if (candidates.Count > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
