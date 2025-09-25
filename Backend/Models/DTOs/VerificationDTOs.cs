namespace Backend.Models.DTOs;

/// <summary>
/// Request til at starte Discord verification
/// </summary>
public class StartDiscordVerificationRequest
{
    /// <summary>
    /// Discord ID der skal verificeres
    /// </summary>
    public string DiscordId { get; set; } = string.Empty;
}

/// <summary>
/// Request til at verificere Discord med kode
/// </summary>
public class VerifyDiscordCodeRequest
{
    /// <summary>
    /// Discord ID der verificeres
    /// </summary>
    public string DiscordId { get; set; } = string.Empty;

    /// <summary>
    /// 6-cifret verification code
    /// </summary>
    public string VerificationCode { get; set; } = string.Empty;
}

/// <summary>
/// Response efter start af verification
/// </summary>
public class StartVerificationResponse
{
    /// <summary>
    /// Om anmodningen var succesfuld
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Besked til brugeren
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Discord brugeren Id der skal verificeres
    /// </summary>
    public string? DiscordUserId { get; set; }

    /// <summary>
    /// Hvornår koden udløber
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
} 