namespace Backend.Models;

public class XpConfig
{
    public Dictionary<string, int> ActivityRewards { get; set; } = new Dictionary<string, int>();
    public Dictionary<string, int> ActivityCooldowns { get; set; } = new Dictionary<string, int>();
    public Dictionary<string, int> DailyLimits { get; set; } = new Dictionary<string, int>();
    public int BaseXP { get; set; } = 100;
    public double LevelMultiplier { get; set; } = 1.5;
}