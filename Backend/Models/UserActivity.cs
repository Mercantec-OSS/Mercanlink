namespace Backend.Models;

public class UserActivity : Common
{
    public string DiscordUserId { get; set; } = string.Empty;
    public string ActivityType { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public int XPAwarded { get; set; }
}