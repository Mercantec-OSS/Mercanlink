namespace Backend.Models;

/// <summary>
/// Model til håndtering af Discord verification codes
/// </summary>
public class DiscordVerification : Common
{
    /// <summary>
    /// ID på brugeren der prøver at verificere
    /// </summary>
    public string DiscordUserId { get; set; } = string.Empty;

    /// <summary>
    /// Discord ID der skal verificeres
    /// </summary>
    public string DiscordId { get; set; } = string.Empty;

    /// <summary>
    /// 6-cifret verification code
    /// </summary>
    public string VerificationCode { get; set; } = string.Empty;

    /// <summary>
    /// Hvornår koden udløber (15 minutter)
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Om koden er blevet brugt
    /// </summary>
    public bool IsUsed { get; set; } = false;

    /// <summary>
    /// Om verification er lykkedes
    /// </summary>
    public bool IsVerified { get; set; } = false;
} 