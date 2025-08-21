namespace Backend.Controllers;

using Backend.Models.DTOs;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

/// <summary>
/// Controller til håndtering af authentication (login, registrering, token refresh)
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
    /// Log ind med email/brugernavn og password
    /// </summary>
    /// <param name="request">Login oplysninger</param>
    /// <returns>Access token og refresh token</returns>
    /// <response code="200">Login succesfult</response>
    /// <response code="401">Ugyldige login oplysninger</response>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var result = await _authService.LoginAsync(request);
            if (result == null)
            {
                return Unauthorized(new { message = "Ugyldige login oplysninger" });
            }

            _logger.LogInformation("Bruger {EmailOrUsername} loggede ind", request.EmailOrUsername);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fejl under login for {EmailOrUsername}", request.EmailOrUsername);
            return StatusCode(500, new { message = "Der opstod en fejl under login" });
        }
    }

    /// <summary>
    /// Registrer ny bruger
    /// </summary>
    /// <param name="request">Registrerings oplysninger</param>
    /// <returns>Access token og refresh token for den nye bruger</returns>
    /// <response code="200">Registrering succesfult</response>
    /// <response code="400">Fejl ved registrering (email/brugernavn i brug)</response>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var result = await _authService.RegisterAsync(request);
            if (result == null)
            {
                return BadRequest(new { message = "Registrering fejlede" });
            }

            _logger.LogInformation("Ny bruger registreret: {Email}", request.Email);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fejl under registrering for {Email}", request.Email);
            return StatusCode(500, new { message = "Der opstod en fejl under registrering" });
        }
    }

    /// <summary>
    /// Refresh access token
    /// </summary>
    /// <param name="request">Refresh token</param>
    /// <returns>Nye access og refresh tokens</returns>
    /// <response code="200">Token refresh succesfult</response>
    /// <response code="401">Ugyldig refresh token</response>
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var result = await _authService.RefreshTokenAsync(request.RefreshToken);
            if (result == null)
            {
                return Unauthorized(new { message = "Ugyldig refresh token" });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fejl under token refresh");
            return StatusCode(500, new { message = "Der opstod en fejl under token refresh" });
        }
    }

    /// <summary>
    /// Log ud og revoke refresh token
    /// </summary>
    /// <param name="request">Refresh token der skal revokes</param>
    /// <returns>Logout bekræftelse</returns>
    /// <response code="200">Logout succesfult</response>
    /// <response code="401">Ikke autoriseret</response>
    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        try
        {
            await _authService.LogoutAsync(request.RefreshToken);
            
            var websiteUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("Bruger {WebsiteUserId} loggede ud", websiteUserId);
            
            return Ok(new { message = "Logout succesfuld" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fejl under logout");
            return StatusCode(500, new { message = "Der opstod en fejl under logout" });
        }
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
            var claims = User.Claims.ToDictionary(c => c.Type, c => c.Value);
            
            return Ok(new
            {
                id = claims.GetValueOrDefault(ClaimTypes.NameIdentifier),
                email = claims.GetValueOrDefault(ClaimTypes.Email),
                username = claims.GetValueOrDefault(ClaimTypes.Name),
                roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList()
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
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Ugyldig bruger ID" });
            }

            var result = await _verificationService.StartVerificationAsync(userId, request.DiscordId);
            
            if (result.Success)
            {
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
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
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
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
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
}

/// <summary>
/// Request for refresh token operation
/// </summary>
public class RefreshTokenRequest
{
    /// <summary>
    /// Refresh token der skal bruges til at få nye tokens
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;
} 