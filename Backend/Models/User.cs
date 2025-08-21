namespace Backend.Models;

public class User : Common
{
    public string UserName { get; set; }

    public string DiscordUserId { get; set; } = string.Empty;

    public string WebsiteUserId { get; set; } = string.Empty;

    public string SchoolADUserId { get; set; } = string.Empty;

    public List<string> Roles { get; set; } = new();



    public SchoolADUser SchoolADUser { get; set; }

    public DiscordUser DiscordUser { get; set; }

    public WebsiteUser WebsiteUser { get; set; }
}
public enum UserRole
{
    Student,
    Teacher,
    Admin,
    Developer,
    AdvisoryBoard
}