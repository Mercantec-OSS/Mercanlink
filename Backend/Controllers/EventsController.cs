using Backend.Data;
using Backend.Discord;
using Backend.Models;
using Backend.Models.DTOs;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly EventsService _eventsService;
    private readonly AuthenticatedUserService _authenticatedUserService;
    private readonly EventDiscordAnnouncer _announcer;
    private readonly ILogger<EventsController> _logger;

    public EventsController(
        ApplicationDbContext context,
        EventsService eventsService,
        AuthenticatedUserService authenticatedUserService,
        EventDiscordAnnouncer announcer,
        ILogger<EventsController> logger)
    {
        _context = context;
        _eventsService = eventsService;
        _authenticatedUserService = authenticatedUserService;
        _announcer = announcer;
        _logger = logger;
    }

    /// <summary>
    /// Hent offentlig liste af events. Default: kommende publicerede events.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<EventListItemDto>>> GetEvents(
        [FromQuery] string? status = "upcoming",
        [FromQuery] EventType? type = null)
    {
        bool? upcoming = status?.ToLowerInvariant() switch
        {
            "upcoming" => true,
            "past" => false,
            "all" => null,
            _ => true
        };

        var events = await _eventsService.ListAsync(upcoming, type, includeUnpublished: false);
        var counts = await _context.EventRegistrations
            .AsNoTracking()
            .GroupBy(r => r.EventId)
            .Select(g => new { EventId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.EventId, x => x.Count);

        var dtos = events.Select(e => _eventsService.MapList(e, counts.GetValueOrDefault(e.Id, 0))).ToList();
        return Ok(dtos);
    }

    /// <summary>
    /// Hent enkelt event via slug.
    /// </summary>
    [HttpGet("{slug}")]
    [AllowAnonymous]
    public async Task<ActionResult<EventDetailDto>> GetEvent(string slug)
    {
        var ev = await _eventsService.GetBySlugAsync(slug);
        if (ev == null)
        {
            return NotFound(new { message = "Event ikke fundet." });
        }

        if (ev.Status == EventStatus.Draft)
        {
            return NotFound(new { message = "Event ikke fundet." });
        }

        var count = await _eventsService.CountRegistrationsAsync(ev.Id);
        return Ok(_eventsService.MapDetail(ev, count));
    }

    /// <summary>
    /// Hent tilmeldte til et event. Kun for indloggede brugere.
    /// </summary>
    [HttpGet("{slug}/registrations")]
    [Authorize]
    public async Task<ActionResult<List<EventRegistrationDto>>> GetRegistrations(string slug)
    {
        var ev = await _eventsService.GetBySlugAsync(slug);
        if (ev == null || ev.Status == EventStatus.Draft)
        {
            return NotFound(new { message = "Event ikke fundet." });
        }

        var registrations = await _context.EventRegistrations
            .AsNoTracking()
            .Where(r => r.EventId == ev.Id)
            .OrderBy(r => r.CreatedAt)
            .Select(r => new EventRegistrationDto
            {
                DisplayName = r.ConfirmedDisplayName,
                Email = r.ConfirmedEmail,
                RegisteredAt = r.CreatedAt,
                Source = r.Source.ToString()
            })
            .ToListAsync();

        return Ok(registrations);
    }

    /// <summary>
    /// Hent ICS-kalenderfil for et event.
    /// </summary>
    [HttpGet("{slug}/ics")]
    [AllowAnonymous]
    public async Task<IActionResult> GetIcs(string slug)
    {
        var ev = await _eventsService.GetBySlugAsync(slug);
        if (ev == null || ev.Status == EventStatus.Draft)
        {
            return NotFound();
        }

        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var ics = _eventsService.BuildIcs(ev, baseUrl);
        return File(System.Text.Encoding.UTF8.GetBytes(ics), "text/calendar", $"{ev.Slug}.ics");
    }

    /// <summary>
    /// Hent indlogget brugers tilmeldinger.
    /// </summary>
    [HttpGet("my-registrations")]
    [Authorize]
    public async Task<ActionResult<List<MyEventRegistrationDto>>> GetMyRegistrations()
    {
        var user = await ResolveOrProvisionUserAsync();
        if (user == null)
        {
            return Unauthorized(new { message = "Kunne ikke finde eller oprette den indloggede bruger." });
        }

        var registrations = await _context.EventRegistrations
            .AsNoTracking()
            .Include(r => r.Event)
            .Where(r => r.UserId == user.Id)
            .OrderByDescending(r => r.Event.StartsAt)
            .Select(r => new MyEventRegistrationDto
            {
                EventId = r.EventId,
                Slug = r.Event.Slug,
                Title = r.Event.Title,
                StartsAt = r.Event.StartsAt,
                RegisteredAt = r.CreatedAt
            })
            .ToListAsync();

        return Ok(registrations);
    }

    /// <summary>
    /// Tilmeld dig et event.
    /// </summary>
    [HttpPost("{slug}/register")]
    [Authorize]
    public async Task<IActionResult> Register(string slug, [FromBody] RegisterForEventRequest request)
    {
        var ev = await _eventsService.GetBySlugAsync(slug);
        if (ev == null || ev.Status == EventStatus.Draft)
        {
            return NotFound(new { message = "Event ikke fundet." });
        }

        var user = await ResolveOrProvisionUserAsync();
        if (user == null)
        {
            return Unauthorized(new { message = "Kunne ikke finde eller oprette den indloggede bruger." });
        }

        var outcome = await _eventsService.RegisterAsync(
            ev,
            user,
            request.ConfirmedDisplayName,
            request.ConfirmedEmail,
            EventRegistrationSource.Web);

        return outcome.Result switch
        {
            EventRegistrationResult.Success => Ok(new { message = "Tilmelding registreret." }),
            EventRegistrationResult.AlreadyRegistered => Conflict(new { message = outcome.Message }),
            EventRegistrationResult.Full => Conflict(new { message = outcome.Message }),
            EventRegistrationResult.DeadlinePassed => BadRequest(new { message = outcome.Message }),
            EventRegistrationResult.NotPublished => BadRequest(new { message = outcome.Message }),
            EventRegistrationResult.InvalidEmail => BadRequest(new { message = outcome.Message }),
            _ => StatusCode(500, new { message = "Ukendt fejl ved tilmelding." })
        };
    }

    /// <summary>
    /// Frameld dig et event.
    /// </summary>
    [HttpDelete("{slug}/register")]
    [Authorize]
    public async Task<IActionResult> Unregister(string slug)
    {
        var ev = await _eventsService.GetBySlugAsync(slug);
        if (ev == null)
        {
            return NotFound(new { message = "Event ikke fundet." });
        }

        var user = await ResolveOrProvisionUserAsync();
        if (user == null)
        {
            return Unauthorized(new { message = "Kunne ikke finde den indloggede bruger." });
        }

        var ok = await _eventsService.UnregisterAsync(ev.Id, user.Id);
        if (!ok)
        {
            return NotFound(new { message = "Du er ikke tilmeldt dette event." });
        }
        return Ok(new { message = "Frameldning registreret." });
    }

    // ==================== ADMIN ====================

    /// <summary>
    /// Admin: list alle events inkl. drafts.
    /// </summary>
    [HttpGet("admin/all")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult<List<EventListItemDto>>> GetAllForAdmin()
    {
        var events = await _eventsService.ListAsync(upcoming: null, type: null, includeUnpublished: true);
        var counts = await _context.EventRegistrations
            .AsNoTracking()
            .GroupBy(r => r.EventId)
            .Select(g => new { EventId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.EventId, x => x.Count);
        var dtos = events.Select(e => _eventsService.MapList(e, counts.GetValueOrDefault(e.Id, 0))).ToList();
        return Ok(dtos);
    }

    /// <summary>
    /// Admin: hent ét event med fuld tekst (inkl. Draft) til redigering.
    /// </summary>
    [HttpGet("admin/detail/{id}")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult<EventDetailDto>> GetEventForAdmin(string id)
    {
        var ev = await _eventsService.GetByIdAsync(id, tracked: false);
        if (ev == null)
        {
            return NotFound(new { message = "Event ikke fundet." });
        }

        var count = await _eventsService.CountRegistrationsAsync(ev.Id);
        return Ok(_eventsService.MapDetail(ev, count));
    }

    /// <summary>
    /// Admin: opret nyt event (status sættes til Draft).
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult<EventDetailDto>> CreateEvent([FromBody] CreateEventRequest request)
    {
        if (request.EndsAt <= request.StartsAt)
        {
            return BadRequest(new { message = "EndsAt skal være efter StartsAt." });
        }

        var creator = await ResolveOrProvisionUserAsync();
        var slugBase = !string.IsNullOrWhiteSpace(request.Slug)
            ? _eventsService.GenerateSlug(request.Slug!)
            : _eventsService.GenerateSlug(request.Title);
        var slug = await _eventsService.EnsureUniqueSlugAsync(slugBase);

        var ev = new Event
        {
            Title = request.Title.Trim(),
            Slug = slug,
            Description = request.Description.Trim(),
            Type = request.Type,
            Status = EventStatus.Draft,
            StartsAt = DateTime.SpecifyKind(request.StartsAt, DateTimeKind.Utc),
            EndsAt = DateTime.SpecifyKind(request.EndsAt, DateTimeKind.Utc),
            Location = request.Location.Trim(),
            LocationUrl = request.LocationUrl,
            BannerImageUrl = request.BannerImageUrl,
            Capacity = request.Capacity,
            RegistrationDeadline = request.RegistrationDeadline.HasValue
                ? DateTime.SpecifyKind(request.RegistrationDeadline.Value, DateTimeKind.Utc)
                : null,
            BringOwnPc = request.BringOwnPc,
            SpeakerName = request.SpeakerName,
            Prerequisites = request.Prerequisites,
            TeamSize = request.TeamSize,
            CreatedByUserId = creator?.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Events.Add(ev);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Event {EventId} oprettet af bruger {UserId}", ev.Id, creator?.Id);
        return CreatedAtAction(nameof(GetEvent), new { slug = ev.Slug }, _eventsService.MapDetail(ev, 0));
    }

    /// <summary>
    /// Admin: opdater eksisterende event.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult<EventDetailDto>> UpdateEvent(string id, [FromBody] UpdateEventRequest request)
    {
        if (request.EndsAt <= request.StartsAt)
        {
            return BadRequest(new { message = "EndsAt skal være efter StartsAt." });
        }

        var ev = await _context.Events.FirstOrDefaultAsync(e => e.Id == id);
        if (ev == null)
        {
            return NotFound(new { message = "Event ikke fundet." });
        }

        ev.Title = request.Title.Trim();
        ev.Description = request.Description.Trim();
        ev.Type = request.Type;
        ev.StartsAt = DateTime.SpecifyKind(request.StartsAt, DateTimeKind.Utc);
        ev.EndsAt = DateTime.SpecifyKind(request.EndsAt, DateTimeKind.Utc);
        ev.Location = request.Location.Trim();
        ev.LocationUrl = request.LocationUrl;
        ev.BannerImageUrl = request.BannerImageUrl;
        ev.Capacity = request.Capacity;
        ev.RegistrationDeadline = request.RegistrationDeadline.HasValue
            ? DateTime.SpecifyKind(request.RegistrationDeadline.Value, DateTimeKind.Utc)
            : null;
        ev.BringOwnPc = request.BringOwnPc;
        ev.SpeakerName = request.SpeakerName;
        ev.Prerequisites = request.Prerequisites;
        ev.TeamSize = request.TeamSize;
        ev.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(request.Slug))
        {
            var newSlugBase = _eventsService.GenerateSlug(request.Slug!);
            ev.Slug = await _eventsService.EnsureUniqueSlugAsync(newSlugBase, ev.Id);
        }

        await _context.SaveChangesAsync();

        if (ev.Status == EventStatus.Published)
        {
            try
            {
                await _announcer.UpdateAnnouncementAsync(ev);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Kunne ikke opdatere Discord-announcement for event {EventId}", ev.Id);
            }
        }

        var count = await _eventsService.CountRegistrationsAsync(ev.Id);
        return Ok(_eventsService.MapDetail(ev, count));
    }

    /// <summary>
    /// Admin: publicer event og send Discord-annoncering.
    /// </summary>
    [HttpPost("{id}/publish")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult<EventDetailDto>> Publish(string id)
    {
        var ev = await _context.Events.FirstOrDefaultAsync(e => e.Id == id);
        if (ev == null)
        {
            return NotFound(new { message = "Event ikke fundet." });
        }

        ev.Status = EventStatus.Published;
        ev.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        try
        {
            await _announcer.PublishAsync(ev);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Discord-annoncering fejlede for event {EventId}", ev.Id);
        }

        var count = await _eventsService.CountRegistrationsAsync(ev.Id);
        return Ok(_eventsService.MapDetail(ev, count));
    }

    /// <summary>
    /// Admin: aflys event.
    /// </summary>
    [HttpPost("{id}/cancel")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult<EventDetailDto>> Cancel(string id)
    {
        var ev = await _context.Events.FirstOrDefaultAsync(e => e.Id == id);
        if (ev == null)
        {
            return NotFound(new { message = "Event ikke fundet." });
        }

        ev.Status = EventStatus.Cancelled;
        ev.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        try
        {
            await _announcer.CancelAsync(ev);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Kunne ikke opdatere Discord ved aflysning af event {EventId}", ev.Id);
        }

        var count = await _eventsService.CountRegistrationsAsync(ev.Id);
        return Ok(_eventsService.MapDetail(ev, count));
    }

    /// <summary>
    /// Admin: slet event permanent.
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> Delete(string id)
    {
        var ev = await _context.Events.FirstOrDefaultAsync(e => e.Id == id);
        if (ev == null)
        {
            return NotFound(new { message = "Event ikke fundet." });
        }

        _context.Events.Remove(ev);
        await _context.SaveChangesAsync();

        try
        {
            await _announcer.CancelAsync(ev);
        }
        catch
        {
        }

        return NoContent();
    }

    /// <summary>
    /// Admin: hent tilmeldinger med fulde detaljer.
    /// </summary>
    [HttpGet("{id}/admin-registrations")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult<List<EventRegistrationDto>>> GetAdminRegistrations(string id)
    {
        var ev = await _context.Events.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
        if (ev == null)
        {
            return NotFound(new { message = "Event ikke fundet." });
        }

        var registrations = await _context.EventRegistrations
            .AsNoTracking()
            .Where(r => r.EventId == ev.Id)
            .OrderBy(r => r.CreatedAt)
            .Select(r => new EventRegistrationDto
            {
                DisplayName = r.ConfirmedDisplayName,
                Email = r.ConfirmedEmail,
                RegisteredAt = r.CreatedAt,
                Source = r.Source.ToString()
            })
            .ToListAsync();

        return Ok(registrations);
    }

    private async Task<User?> ResolveOrProvisionUserAsync()
    {
        var user = await _authenticatedUserService.ResolveCurrentUserAsync(User);
        if (user == null)
        {
            user = await _authenticatedUserService.ProvisionUserFromClaimsAsync(User);
        }
        return user;
    }
}
