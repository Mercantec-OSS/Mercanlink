namespace Backend.DiscordServices.Services;

using Backend.Models;
using Microsoft.Extensions.Options;

public class LevelSystem
{
    private readonly XPConfig _xpConfig;

    public LevelSystem(IOptions<XPConfig> xpConfig)
    {
        _xpConfig = xpConfig.Value;
    }

    public int CalculateRequiredXP(int level)
    {
        return (int)(_xpConfig.BaseXP * Math.Pow(level, _xpConfig.LevelMultiplier));
    }

    public (int newLevel, bool didLevelUp) CalculateLevel(int currentLevel, int totalXP)
    {
        int requiredXP = CalculateRequiredXP(currentLevel);

        if (totalXP >= requiredXP)
        {
            return (currentLevel + 1, true);
        }

        return (currentLevel, false);
    }

    public int GetXPForActivity(XPActivityType activity)
    {
        string activityName = activity.ToString();
        if (_xpConfig.ActivityRewards.TryGetValue(activityName, out int xpAmount))
        {
            return xpAmount;
        }
        return 0;
    }
}

public enum XPActivityType
{
    Message,
    Reaction,
    VoiceMinute,
    DailyLogin,
    CommandUsed
}