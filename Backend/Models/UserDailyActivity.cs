namespace Backend.Models;

public class UserDailyActivity : Common
{
    public string UserId { get; set; } = string.Empty;
    public string ActivityType { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.UtcNow.Date;
    public int Count { get; set; } = 0;
    public int TotalXPAwarded { get; set; } = 0;
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;
}