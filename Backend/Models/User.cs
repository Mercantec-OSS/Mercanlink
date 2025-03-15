namespace Backend.Models;

public class User : Common
{
    // Basis bruger information
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }

    // Discord integration (optional)
    public string? DiscordId { get; set; }
    public string? GlobalName { get; set; }
    public string? Discriminator { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Nickname { get; set; }
    public bool? IsBot { get; set; }
    public int? PublicFlags { get; set; }
    public DateTime? JoinedAt { get; set; }
    public bool? IsBoosting { get; set; }

    // Sikkerhed og roller
    public List<string> Roles { get; set; } = new();

    // Metadata
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}

public enum UserRole
{
    Student,
    Teacher,
    Admin,
    Developer,
    AdvisoryBoard
}
