namespace Backend.Services;

using Backend.Data;
using Backend.DBAccess;
using Backend.Models;
using Backend.Models.DTOs;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Service til håndtering af Discord verification
/// </summary>
public class DiscordVerificationService
{
    private readonly DiscordVerificationDBAccess _discordVerificationDBAccess;
    private readonly ILogger<DiscordVerificationService> _logger;

    public DiscordVerificationService(ILogger<DiscordVerificationService> logger, DiscordVerificationDBAccess discordVerificationDBAccess)
    {
        _logger = logger;
        _discordVerificationDBAccess = discordVerificationDBAccess;
    }

    /// <summary>
    /// Starter Discord verification proces
    /// </summary>
    public async Task<StartVerificationResponse> StartVerificationAsync(string userId, string discordId)
    {
        try
        {
            if (_discordVerificationDBAccess.CheckExistingUser(discordId) != null)
            {
                return new StartVerificationResponse
                {
                    Success = false,
                    Message = "Denne Discord konto er allerede tilknyttet en anden bruger"
                };
            }

            await _discordVerificationDBAccess.RemoveExistingVerifications(userId, discordId);

            // Generer 6-cifret kode
            var verificationCode = GenerateVerificationCode();
            var expiresAt = DateTime.UtcNow.AddMinutes(15);

            // Opret ny verification
            var verification = new DiscordVerification
            {
                UserId = userId,
                DiscordId = discordId,
                VerificationCode = verificationCode,
                ExpiresAt = expiresAt,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _discordVerificationDBAccess.AddVerification(verification);

            _logger.LogInformation("Discord verification startet for bruger {UserId} og Discord {DiscordId}", userId, discordId);

            return new StartVerificationResponse
            {
                Success = true,
                Message = "Verification kode sendt til din Discord konto",
                ExpiresAt = expiresAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fejl ved start af Discord verification");
            return new StartVerificationResponse
            {
                Success = false,
                Message = "Der opstod en fejl ved start af verification"
            };
        }
    }

    /// <summary>
    /// Verificerer Discord kode
    /// </summary>
    public async Task<bool> VerifyCodeAsync(string userId, string discordId, string code)
    {
        try
        {
            var verification = await _discordVerificationDBAccess.CheckVerificationCode(userId, discordId, code);

            if (verification == null)
            {
                _logger.LogWarning("Ugyldig verification kode for bruger {UserId} og Discord {DiscordId}", userId, discordId);
                return false;
            }

            // Marker som brugt og verificeret
            verification.IsUsed = true;
            verification.IsVerified = true;
            verification.UpdatedAt = DateTime.UtcNow;

            await _discordVerificationDBAccess.UpdateVerificationCode(verification);

            _logger.LogInformation("Discord verification gennemført for bruger {UserId} og Discord {DiscordId}", userId, discordId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fejl ved verification af Discord kode");
            return false;
        }
    }

    /// <summary>
    /// Henter aktiv verification for en bruger
    /// </summary>
    public async Task<DiscordVerification?> GetActiveVerificationAsync(string userId, string discordId)
    {
        return await _discordVerificationDBAccess.GetActiveVerification(userId, discordId);
    }

    /// <summary>
    /// Rydder op i udløbne verification koder
    /// </summary>
    public async Task CleanupExpiredVerificationsAsync()
    {
        var expiredCount = await _discordVerificationDBAccess.CleanupExpiredVerifications();

        if (expiredCount > 0)
        {
            _logger.LogInformation("Fjernede {Count} udløbne Discord verifications", expiredCount);
        }
    }

    /// <summary>
    /// Genererer en 6-cifret verification kode
    /// </summary>
    private static string GenerateVerificationCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }
} 