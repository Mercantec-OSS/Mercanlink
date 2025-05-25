namespace Backend.Services;

using Backend.Data;
using Backend.Models;
using Backend.Models.DTOs;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Service til håndtering af Discord verification
/// </summary>
public class DiscordVerificationService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DiscordVerificationService> _logger;

    public DiscordVerificationService(ApplicationDbContext context, ILogger<DiscordVerificationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Starter Discord verification proces
    /// </summary>
    public async Task<StartVerificationResponse> StartVerificationAsync(string userId, string discordId)
    {
        try
        {
            // Tjek om Discord ID allerede er verificeret
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.DiscordId == discordId && !string.IsNullOrEmpty(u.Email));

            if (existingUser != null)
            {
                return new StartVerificationResponse
                {
                    Success = false,
                    Message = "Denne Discord konto er allerede tilknyttet en anden bruger"
                };
            }

            // Slet eventuelt eksisterende verification for denne bruger og Discord ID
            var existingVerifications = await _context.DiscordVerifications
                .Where(dv => (dv.UserId == userId || dv.DiscordId == discordId) && !dv.IsUsed)
                .ToListAsync();

            if (existingVerifications.Any())
            {
                _context.DiscordVerifications.RemoveRange(existingVerifications);
            }

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

            await _context.DiscordVerifications.AddAsync(verification);
            await _context.SaveChangesAsync();

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
            var verification = await _context.DiscordVerifications
                .FirstOrDefaultAsync(dv => 
                    dv.UserId == userId && 
                    dv.DiscordId == discordId && 
                    dv.VerificationCode == code &&
                    !dv.IsUsed &&
                    dv.ExpiresAt > DateTime.UtcNow);

            if (verification == null)
            {
                _logger.LogWarning("Ugyldig verification kode for bruger {UserId} og Discord {DiscordId}", userId, discordId);
                return false;
            }

            // Marker som brugt og verificeret
            verification.IsUsed = true;
            verification.IsVerified = true;
            verification.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

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
        return await _context.DiscordVerifications
            .FirstOrDefaultAsync(dv => 
                dv.UserId == userId && 
                dv.DiscordId == discordId && 
                !dv.IsUsed &&
                dv.ExpiresAt > DateTime.UtcNow);
    }

    /// <summary>
    /// Rydder op i udløbne verification koder
    /// </summary>
    public async Task CleanupExpiredVerificationsAsync()
    {
        var expired = await _context.DiscordVerifications
            .Where(dv => dv.ExpiresAt <= DateTime.UtcNow || dv.CreatedAt < DateTime.UtcNow.AddDays(-1))
            .ToListAsync();

        if (expired.Any())
        {
            _context.DiscordVerifications.RemoveRange(expired);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Fjernede {Count} udløbne Discord verifications", expired.Count);
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