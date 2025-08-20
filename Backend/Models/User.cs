namespace Backend.Models;

public class User : Common
{
    public string UserName { get; set; }

    public string DiscordUserId { get; set; } = string.Empty;

    public string WebsiteUserId { get; set; } = string.Empty;

    public string SchoolADUserId { get; set; } = string.Empty;



    public SchoolADUser SchoolADUser { get; set; }

    public DiscordUser DiscordUser { get; set; }

    public WebsiteUser WebsiteUser { get; set; }
}