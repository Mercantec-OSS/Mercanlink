namespace Backend.DiscordServices.Services;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Backend.DBAccess;

public class XPService
{
    private readonly ILogger<XPService> _logger;
    private readonly DiscordBotService _discordService;
    private readonly LevelSystem _levelSystem;
    private readonly XPConfig _xpConfig;
    private readonly DiscordBotDBAccess _discordBotDBAccess;

    public XPService(
        DiscordBotDBAccess discordBotDBAccess,
        ILogger<XPService> logger,
        DiscordBotService discordService,
        LevelSystem levelSystem,
        IOptions<XPConfig> xpConfig)
    {
        _discordService = discordService;
        _logger = logger;
        _discordService = discordService;
        _levelSystem = levelSystem;
        _xpConfig = xpConfig.Value;
    }

    public async Task<bool> AddXPAsync(string discordId, XPActivityType activity)
    {
        _logger.LogInformation("Forsøger at tilføje XP for aktivitet {Activity} til bruger {DiscordId}", activity, discordId);

        var user = await _discordBotDBAccess.GetUser(discordId);
        if (user == null)
        {
            _logger.LogWarning("Bruger med Discord ID {DiscordId} blev ikke fundet", discordId);
            return false;
        }

        string activityName = activity.ToString();
        var today = DateTime.UtcNow.Date;

        _logger.LogInformation("Søger efter dagens aktivitet for bruger {UserId}, aktivitet {Activity}", user.Id, activityName);

        // Find eller opret dagens aktivitetspost
        var dailyActivity = await _discordBotDBAccess.CheckTodaysActivity(user.Id, activityName, today);

        bool isFirstActivity = false;

        if (dailyActivity == null)
        {
            _logger.LogInformation("Opretter ny daglig aktivitetspost for bruger {UserId}, aktivitet {Activity}", user.Id, activityName);

            isFirstActivity = true; // Dette er brugerens første aktivitet af denne type i dag

            dailyActivity = new UserDailyActivity
            {
                UserId = user.Id,
                ActivityType = activityName,
                Date = today,
                Count = 0,
                TotalXPAwarded = 0,
                LastActivity = DateTime.UtcNow
            };

            // Gem aktivitetsposten med det samme
            try
            {
                await _discordBotDBAccess.AddDailyActivity(dailyActivity);
                _logger.LogInformation("Gemt ny aktivitetspost for bruger {UserId}, aktivitet {Activity}", user.Id, activityName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved gemning af ny aktivitetspost for bruger {UserId}, aktivitet {Activity}", user.Id, activityName);
                return false;
            }
        }

        // Hvis det er første aktivitet, skal vi ikke tjekke cooldown
        if (!isFirstActivity)
        {
            // Tjek cooldown
            if (_xpConfig.ActivityCooldowns.TryGetValue(activityName, out int cooldownSeconds) && cooldownSeconds > 0)
            {
                var timeSinceLastActivity = DateTime.UtcNow - dailyActivity.LastActivity;
                if (timeSinceLastActivity.TotalSeconds < cooldownSeconds)
                {
                    _logger.LogInformation("Cooldown aktiv for bruger {UserId}, aktivitet {Activity}. Mangler {Seconds} sekunder",
                        user.Id, activityName, cooldownSeconds - (int)timeSinceLastActivity.TotalSeconds);
                    return false;
                }
            }

            // Tjek daglig grænse
            if (_xpConfig.DailyLimits.TryGetValue(activityName, out int dailyLimit) && dailyLimit > 0)
            {
                if (dailyActivity.Count >= dailyLimit)
                {
                    _logger.LogInformation("Daglig grænse nået for bruger {UserId}, aktivitet {Activity}", user.Id, activityName);
                    return false;
                }
            }
        }

        // Beregn XP
        int xpToAdd = _levelSystem.GetXPForActivity(activity);
        if (xpToAdd <= 0) return false;

        // Opdater aktivitetsdata
        dailyActivity.Count++;
        dailyActivity.TotalXPAwarded += xpToAdd;
        dailyActivity.LastActivity = DateTime.UtcNow;

        // Tilføj XP til brugeren
        user.Experience += xpToAdd;

        // Tjek for level up
        var (newLevel, didLevelUp) = _levelSystem.CalculateLevel(user.Level, user.Experience);

        if (didLevelUp)
        {
            user.Level = newLevel;
            await _discordService.SendLevelUpMessage(discordId, newLevel);
            _logger.LogInformation("User {DiscordId} leveled up to {Level}", discordId, newLevel);
        }

        await _discordBotDBAccess.UpdateDailyAcitivity(dailyActivity);
        return true;
    }

    public async Task<(int Level, int XP, int RequiredXP)> GetUserProgressAsync(string discordId)
    {
        var user = await _discordBotDBAccess.GetUser(discordId);
        if (user == null) return (0, 0, 0);

        int requiredXP = _levelSystem.CalculateRequiredXP(user.Level);
        return (user.Level, user.Experience, requiredXP);
    }

    public async Task<Dictionary<string, int>> GetUserActivityStatsAsync(string discordId)
    {
        var user = await _discordBotDBAccess.GetUser(discordId);
        if (user == null) return new Dictionary<string, int>();

        var today = DateTime.UtcNow.Date;
        var stats = new Dictionary<string, int>();

        var dailyActivities = await _discordBotDBAccess.GetAllTodaysActivity(user.Id, today);

        foreach (var activityType in Enum.GetNames(typeof(XPActivityType)))
        {
            var activity = dailyActivities.FirstOrDefault(a => a.ActivityType == activityType);
            stats[activityType] = activity?.Count ?? 0;
        }

        return stats;
    }

    public async Task<bool> CheckAndAwardDailyLoginAsync(string discordId)
    {
        // Tjek om brugeren allerede har fået daglig bonus i dag
        var user = await _discordBotDBAccess.GetUser(discordId);
        if (user == null) return false;

        var today = DateTime.UtcNow.Date;

        // Tjek om brugeren allerede har fået DailyLogin XP i dag
        var dailyLoginActivity = await _discordBotDBAccess.CheckIfDailyLoginXPIsRewarded(user.Id, today);

        // Hvis brugeren ikke har fået daglig bonus endnu, giv den nu
        if (dailyLoginActivity == null || dailyLoginActivity.Count == 0)
        {
            _logger.LogInformation("Tildeler daglig login bonus til bruger {UserId}", user.Id);
            return await AddXPAsync(discordId, XPActivityType.DailyLogin);
        }

        return false;
    }
}