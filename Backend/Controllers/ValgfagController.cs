using Backend.Config;
using Backend.Data;
using Backend.Models;
using Backend.Models.DTOs;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ValgfagController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly AuthenticatedUserService _authenticatedUserService;
    private readonly ILogger<ValgfagController> _logger;

    public ValgfagController(
        ApplicationDbContext context,
        AuthenticatedUserService authenticatedUserService,
        ILogger<ValgfagController> logger
    )
    {
        _context = context;
        _authenticatedUserService = authenticatedUserService;
        _logger = logger;
    }

    [HttpGet("enrollments")]
    [AllowAnonymous]
    public async Task<ActionResult<List<ElectiveEnrollmentsGroupDto>>> GetEnrollments()
    {
        var rows = await _context.ElectiveEnrollments
            .AsNoTracking()
            .Include(e => e.User)
            .ThenInclude(u => u.DiscordUser)
            .Include(e => e.User)
            .ThenInclude(u => u.WebsiteUser)
            .OrderBy(e => e.ElectiveKey)
            .ThenBy(e => e.CreatedAt)
            .ToListAsync();

        var grouped = rows
            .GroupBy(e => e.ElectiveKey)
            .Select(g => new ElectiveEnrollmentsGroupDto
            {
                ElectiveKey = g.Key,
                Participants = g
                    .Select(e => new ElectiveEnrollmentParticipantDto
                    {
                        DisplayName = AuthenticatedUserService.ResolveDisplayName(e.User),
                        EnrolledAt = e.CreatedAt
                    })
                    .OrderBy(p => p.DisplayName, StringComparer.OrdinalIgnoreCase)
                    .ToList()
            })
            .OrderBy(g => g.ElectiveKey)
            .ToList();

        return Ok(grouped);
    }

    [HttpGet("my-enrollments")]
    [Authorize]
    public async Task<ActionResult<List<string>>> GetMyEnrollments()
    {
        var user = await _authenticatedUserService.ResolveCurrentUserAsync(User);
        if (user == null)
        {
            user = await _authenticatedUserService.ProvisionUserFromClaimsAsync(User);
            if (user == null)
                return Unauthorized(new { message = "Kunne ikke finde eller oprette den indloggede bruger i systemet." });
        }

        var keys = await _context.ElectiveEnrollments
            .AsNoTracking()
            .Where(e => e.UserId == user.Id)
            .Select(e => e.ElectiveKey)
            .Distinct()
            .OrderBy(k => k)
            .ToListAsync();

        return Ok(keys);
    }

    [HttpPost("enroll")]
    [Authorize]
    public async Task<ActionResult> Enroll([FromBody] EnrollElectiveRequest request)
    {
        var key = request.ElectiveKey?.Trim();
        if (string.IsNullOrEmpty(key))
            return BadRequest(new { message = "electiveKey er påkrævet." });

        if (!ElectiveDefinitions.IsValidAndOpen(key, DateTime.UtcNow))
            return BadRequest(new { message = "Ugyldigt eller lukket valgfag." });

        var user = await _authenticatedUserService.ResolveCurrentUserAsync(User);
        if (user == null)
        {
            user = await _authenticatedUserService.ProvisionUserFromClaimsAsync(User);
            if (user == null)
                return Unauthorized(new { message = "Kunne ikke finde eller oprette den indloggede bruger i systemet." });
        }

        var exists = await _context.ElectiveEnrollments.AnyAsync(e => e.ElectiveKey == key && e.UserId == user.Id);
        if (exists)
            return Conflict(new { message = "Du er allerede tilmeldt dette valgfag." });

        var enrollment = new ElectiveEnrollment
        {
            ElectiveKey = key,
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.ElectiveEnrollments.Add(enrollment);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogWarning(ex, "Dobbelttilmelding valgfag {Key} bruger {UserId}", key, user.Id);
            return Conflict(new { message = "Du er allerede tilmeldt dette valgfag." });
        }

        _logger.LogInformation("Bruger {UserId} tilmeldt valgfag {Key}", user.Id, key);
        return Ok(new { message = "Tilmelding registreret." });
    }
}
