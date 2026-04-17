namespace Backend.Controllers;

using Backend.Models.DTOs;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

/// <summary>
/// Controller til brugerinformation og Discord-linking for allerede autentificerede brugere.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly DiscordVerificationService _verificationService;
    private readonly DiscordBotService _discordBotService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(AuthService authService, DiscordVerificationService verificationService, DiscordBotService discordBotService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _verificationService = verificationService;
        _discordBotService = discordBotService;
        _logger = logger;
    }

    /// <summary>
    /// Hent information om den nuværende indloggede bruger
    /// </summary>
    /// <returns>Bruger information fra JWT token</returns>
    /// <response code="200">Bruger information</response>
    /// <response code="401">Ikke autoriseret</response>
    [HttpGet("me")]
    [Authorize]
    public ActionResult<object> GetCurrentUser()
    {
        try
        {
            var claims = User.Claims.GroupBy(c => c.Type).ToDictionary(group => group.Key, group => group.First().Value);

            return Ok(new
            {
                userid = GetCurrentUserId(),
                sub = claims.GetValueOrDefault("sub"),
                email = claims.GetValueOrDefault(ClaimTypes.Email),
                username = claims.GetValueOrDefault(ClaimTypes.Name) ?? claims.GetValueOrDefault("name"),
                roles = User.Claims
                    .Where(c =>
                        c.Type == ClaimTypes.Role
                        || c.Type == "role"
                        || c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
                    .Select(c => c.Value)
                    .Distinct()
                    .ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fejl under hentning af bruger information");
            return StatusCode(500, new { message = "Der opstod en fejl" });
        }
    }

    /// <summary>
    /// Start Discord verification proces
    /// </summary>
    /// <param name="request">Discord ID der skal verificeres</param>
    /// <returns>Verification status</returns>
    /// <response code="200">Verification startet succesfuldt</response>
    /// <response code="400">Fejl ved start af verification</response>
    /// <response code="401">Ikke autoriseret</response>
    [HttpPost("start-discord-verification")]
    [Authorize]
    public async Task<ActionResult<StartVerificationResponse>> StartDiscordVerification([FromBody] StartDiscordVerificationRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Ugyldig bruger ID" });
            }

            var result = await _verificationService.StartVerificationAsync(userId, request.DiscordId);
            
            if (result.Success)
            {
                if (string.IsNullOrEmpty(result.DiscordUserId))
                {
                    return BadRequest(new { message = "Verification kunne ikke startes korrekt." });
                }

                // Hent verification koden fra databasen
                var verification = await _verificationService.GetActiveVerificationAsync(result.DiscordUserId, request.DiscordId);
                if (verification != null)
                {
                    // Send koden via Discord bot
                    var messageSent = await _discordBotService.SendVerificationCodeAsync(request.DiscordId, verification.VerificationCode);
                    if (!messageSent)
                    {
                        return BadRequest(new { message = "Kunne ikke sende besked til Discord brugeren. Tjek at Discord ID'et er korrekt." });
                    }
                }
                
                _logger.LogInformation("Discord verification startet for bruger {UserId} med Discord {DiscordId}", userId, request.DiscordId);
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fejl ved start af Discord verification");
            return StatusCode(500, new { message = "Der opstod en fejl ved start af verification" });
        }
    }

    /// <summary>
    /// Verificer Discord kode og link kontoen
    /// </summary>
    /// <param name="request">Discord ID og verification kode</param>
    /// <returns>Verification resultat</returns>
    /// <response code="200">Verification gennemført succesfuldt</response>
    /// <response code="400">Ugyldig verification kode</response>
    /// <response code="401">Ikke autoriseret</response>
    [HttpPost("verify-discord-code")]
    [Authorize]
    public async Task<ActionResult> VerifyDiscordCode([FromBody] VerifyDiscordCodeRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Ugyldig bruger ID" });
            }
            
            var isValid = await _verificationService.VerifyCodeAsync(userId, request.DiscordId, request.VerificationCode);
            
            if (isValid)
            {
                // Nu link Discord kontoen til brugeren
                var linkSuccess = await _authService.LinkDiscordAsync(userId, request.DiscordId);
                
                if (linkSuccess)
                {
                    _logger.LogInformation("Discord konto {DiscordId} verificeret og linket til bruger {UserId}", request.DiscordId, userId);
                    return Ok(new { message = "Discord konto verificeret og linket succesfuldt" });
                }
                else
                {
                    return BadRequest(new { message = "Discord kontoen kunne ikke linkes" });
                }
            }
            else
            {
                return BadRequest(new { message = "Ugyldig eller udløbet verification kode" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fejl ved verification af Discord kode");
            return StatusCode(500, new { message = "Der opstod en fejl ved verification" });
        }
    }

    /// <summary>
    /// Fjern link mellem Discord og bruger (unlink)
    /// </summary>
    /// <returns>Bekræftelse på unlink</returns>
    /// <response code="200">Unlink succesfuldt</response>
    /// <response code="401">Ikke autoriseret</response>
    [HttpPost("unlink-discord")]
    [Authorize]
    public async Task<ActionResult> UnlinkDiscord()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Ugyldig bruger ID" });
            }

            var success = await _authService.UnlinkDiscordAsync(userId);
            if (success)
            {
                _logger.LogInformation("Discord konto unlinked fra bruger {UserId}", userId);
                return Ok(new { message = "Discord konto er nu fjernet fra din bruger" });
            }
            else
            {
                return BadRequest(new { message = "Der var ikke linket en Discord konto til denne bruger" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fejl under unlink af Discord konto");
            return StatusCode(500, new { message = "Der opstod en fejl under unlink" });
        }
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}