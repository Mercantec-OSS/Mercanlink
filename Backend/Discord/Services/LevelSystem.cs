using Backend.Data;
using Backend.Discord.Enums;
using Microsoft.OpenApi.Extensions;

namespace Backend.DiscordServices.Services;

using Backend.Models;
using Microsoft.Extensions.Options;

public class LevelSystem
{
    private readonly XpConfig _xpConfig;
    private readonly ApplicationDbContext _context;

    public LevelSystem(IOptions<XpConfig> xpConfig, ApplicationDbContext context)
    {
        _xpConfig = xpConfig.Value;
        _context = context;
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

    public int GetXPForActivity(XpActivityType activity)
    {
        return _context.XpRewards.First(reward => reward.Name == activity.GetName()).Reward;
    }
}
