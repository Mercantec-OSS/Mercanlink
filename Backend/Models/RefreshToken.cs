namespace Backend.Models;

public class RefreshToken : Common
{
    public string Token { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public WebsiteUser WebsiteUser { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; } = false;
    public DateTime? RevokedAt { get; set; }
    public string? ReplacedByToken { get; set; }
} 