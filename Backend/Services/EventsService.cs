using System.Text;
using System.Text.RegularExpressions;
using Backend.Data;
using Backend.Models;
using Backend.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public enum EventRegistrationResult
{
    Success,
    EventNotFound,
    NotPublished,
    DeadlinePassed,
    Full,
    AlreadyRegistered,
    InvalidEmail,
    UserNotFound
}

public class EventRegistrationOutcome
{
    public EventRegistrationResult Result { get; init; }
    public string? Message { get; init; }
    public EventRegistration? Registration { get; init; }
}

/// <summary>
/// Forretningslogik for events: opslag, slug-håndtering, tilmelding/frameld og ICS-eksport.
/// Bruges af både EventsController (web) og Discord slash commands.
/// </summary>
public class EventsService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<EventsService> _logger;
    private readonly Regex _eduEmailRegex;

    public EventsService(ApplicationDbContext context, IConfiguration configuration, ILogger<EventsService> logger)
    {
        _context = context;
        _logger = logger;
        var pattern = Environment.GetEnvironmentVariable("EVENTS_EDU_EMAIL_REGEX")
            ?? configuration["Events:EduEmailRegex"]
            ?? @"^[^@\s]+@(edu\.)?mercantec\.dk$";
        _eduEmailRegex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
    }

    public bool IsValidEduEmail(string email)
    {
        return !string.IsNullOrWhiteSpace(email) && _eduEmailRegex.IsMatch(email.Trim());
    }

    public async Task<Event?> GetBySlugAsync(string slug, bool includeRegistrations = false)
    {
        IQueryable<Event> query = _context.Events.AsNoTracking();
        if (includeRegistrations)
        {
            query = query.Include(e => e.Registrations);
        }
        return await query.FirstOrDefaultAsync(e => e.Slug == slug);
    }

    public async Task<Event?> GetByIdAsync(string id, bool tracked = false)
    {
        if (tracked)
        {
            return await _context.Events.FirstOrDefaultAsync(e => e.Id == id);
        }
        return await _context.Events.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<List<Event>> ListAsync(bool? upcoming, EventType? type, bool includeUnpublished)
    {
        var now = DateTime.UtcNow;
        IQueryable<Event> query = _context.Events.AsNoTracking();

        if (!includeUnpublished)
        {
            query = query.Where(e => e.Status == EventStatus.Published || e.Status == EventStatus.Cancelled);
        }

        if (type.HasValue)
        {
            query = query.Where(e => e.Type == type.Value);
        }

        if (upcoming.HasValue)
        {
            query = upcoming.Value
                ? query.Where(e => e.EndsAt >= now).OrderBy(e => e.StartsAt)
                : query.Where(e => e.EndsAt < now).OrderByDescending(e => e.StartsAt);
        }
        else
        {
            query = query.OrderByDescending(e => e.StartsAt);
        }

        return await query.ToListAsync();
    }

    public string GenerateSlug(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return Guid.NewGuid().ToString("n").Substring(0, 8);
        }

        var lower = title.ToLowerInvariant();
        var sb = new StringBuilder(lower.Length);
        foreach (var ch in lower)
        {
            if (ch is >= 'a' and <= 'z' or >= '0' and <= '9')
            {
                sb.Append(ch);
            }
            else if (ch is ' ' or '-' or '_' or '.' or '/')
            {
                sb.Append('-');
            }
            else if (ch == 'æ') sb.Append("ae");
            else if (ch == 'ø') sb.Append("oe");
            else if (ch == 'å') sb.Append("aa");
        }

        var slug = Regex.Replace(sb.ToString(), "-+", "-").Trim('-');
        if (string.IsNullOrEmpty(slug))
        {
            slug = Guid.NewGuid().ToString("n").Substring(0, 8);
        }
        return slug.Length > 140 ? slug.Substring(0, 140) : slug;
    }

    public async Task<string> EnsureUniqueSlugAsync(string baseSlug, string? excludeEventId = null)
    {
        var slug = baseSlug;
        var suffix = 1;
        while (await _context.Events.AnyAsync(e => e.Slug == slug && (excludeEventId == null || e.Id != excludeEventId)))
        {
            suffix++;
            slug = $"{baseSlug}-{suffix}";
        }
        return slug;
    }

    public async Task<EventRegistrationOutcome> RegisterAsync(
        Event ev,
        User user,
        string confirmedDisplayName,
        string confirmedEmail,
        EventRegistrationSource source)
    {
        if (ev.Status != EventStatus.Published)
        {
            return new EventRegistrationOutcome { Result = EventRegistrationResult.NotPublished, Message = "Eventet er ikke åbent for tilmelding." };
        }

        if (ev.RegistrationDeadline.HasValue && DateTime.UtcNow > ev.RegistrationDeadline.Value)
        {
            return new EventRegistrationOutcome { Result = EventRegistrationResult.DeadlinePassed, Message = "Tilmeldingsfristen er overskredet." };
        }

        if (ev.EndsAt < DateTime.UtcNow)
        {
            return new EventRegistrationOutcome { Result = EventRegistrationResult.DeadlinePassed, Message = "Eventet er allerede afsluttet." };
        }

        if (!IsValidEduEmail(confirmedEmail))
        {
            return new EventRegistrationOutcome { Result = EventRegistrationResult.InvalidEmail, Message = "E-mail skal være en gyldig Mercantec edu-mail." };
        }

        var displayName = string.IsNullOrWhiteSpace(confirmedDisplayName)
            ? AuthenticatedUserService.ResolveDisplayName(user)
            : confirmedDisplayName.Trim();

        if (ev.Capacity.HasValue)
        {
            var taken = await _context.EventRegistrations.CountAsync(r => r.EventId == ev.Id);
            if (taken >= ev.Capacity.Value)
            {
                return new EventRegistrationOutcome { Result = EventRegistrationResult.Full, Message = "Eventet er fyldt op." };
            }
        }

        var alreadyRegistered = await _context.EventRegistrations.AnyAsync(r => r.EventId == ev.Id && r.UserId == user.Id);
        if (alreadyRegistered)
        {
            return new EventRegistrationOutcome { Result = EventRegistrationResult.AlreadyRegistered, Message = "Du er allerede tilmeldt." };
        }

        var registration = new EventRegistration
        {
            EventId = ev.Id,
            UserId = user.Id,
            ConfirmedDisplayName = displayName,
            ConfirmedEmail = confirmedEmail.Trim(),
            Source = source,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.EventRegistrations.Add(registration);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogWarning(ex, "Dobbelttilmelding event {EventId} bruger {UserId}", ev.Id, user.Id);
            return new EventRegistrationOutcome { Result = EventRegistrationResult.AlreadyRegistered, Message = "Du er allerede tilmeldt." };
        }

        return new EventRegistrationOutcome { Result = EventRegistrationResult.Success, Registration = registration };
    }

    public async Task<bool> UnregisterAsync(string eventId, string userId)
    {
        var registration = await _context.EventRegistrations
            .FirstOrDefaultAsync(r => r.EventId == eventId && r.UserId == userId);
        if (registration == null)
        {
            return false;
        }
        _context.EventRegistrations.Remove(registration);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> CountRegistrationsAsync(string eventId)
    {
        return await _context.EventRegistrations.AsNoTracking().CountAsync(r => r.EventId == eventId);
    }

    public EventListItemDto MapList(Event ev, int registrationCount)
    {
        return new EventListItemDto
        {
            Id = ev.Id,
            Slug = ev.Slug,
            Title = ev.Title,
            Type = ev.Type.ToString(),
            Status = ev.Status.ToString(),
            StartsAt = ev.StartsAt,
            EndsAt = ev.EndsAt,
            Location = ev.Location,
            BannerImageUrl = ev.BannerImageUrl,
            Capacity = ev.Capacity,
            RegistrationCount = registrationCount,
            RegistrationDeadline = ev.RegistrationDeadline
        };
    }

    public EventDetailDto MapDetail(Event ev, int registrationCount)
    {
        var now = DateTime.UtcNow;
        var deadlineOk = !ev.RegistrationDeadline.HasValue || now <= ev.RegistrationDeadline.Value;
        var notFinished = ev.EndsAt >= now;
        var capacityOk = !ev.Capacity.HasValue || registrationCount < ev.Capacity.Value;

        return new EventDetailDto
        {
            Id = ev.Id,
            Slug = ev.Slug,
            Title = ev.Title,
            Type = ev.Type.ToString(),
            Status = ev.Status.ToString(),
            StartsAt = ev.StartsAt,
            EndsAt = ev.EndsAt,
            Location = ev.Location,
            BannerImageUrl = ev.BannerImageUrl,
            Capacity = ev.Capacity,
            RegistrationCount = registrationCount,
            RegistrationDeadline = ev.RegistrationDeadline,
            Description = ev.Description,
            LocationUrl = ev.LocationUrl,
            BringOwnPc = ev.BringOwnPc,
            SpeakerName = ev.SpeakerName,
            Prerequisites = ev.Prerequisites,
            TeamSize = ev.TeamSize,
            RegistrationOpen = ev.Status == EventStatus.Published && deadlineOk && notFinished && capacityOk,
            IsFull = ev.Capacity.HasValue && registrationCount >= ev.Capacity.Value
        };
    }

    public string BuildIcs(Event ev, string baseUrl)
    {
        string Esc(string? s) => (s ?? string.Empty)
            .Replace("\\", "\\\\")
            .Replace(",", "\\,")
            .Replace(";", "\\;")
            .Replace("\r\n", "\\n")
            .Replace("\n", "\\n");

        string Fmt(DateTime dt) => dt.ToUniversalTime().ToString("yyyyMMdd'T'HHmmss'Z'");

        var sb = new StringBuilder();
        sb.AppendLine("BEGIN:VCALENDAR");
        sb.AppendLine("VERSION:2.0");
        sb.AppendLine("PRODID:-//Mercantec-Space//Events//DA");
        sb.AppendLine("CALSCALE:GREGORIAN");
        sb.AppendLine("METHOD:PUBLISH");
        sb.AppendLine("BEGIN:VEVENT");
        sb.AppendLine($"UID:event-{ev.Id}@mercantec-space");
        sb.AppendLine($"DTSTAMP:{Fmt(DateTime.UtcNow)}");
        sb.AppendLine($"DTSTART:{Fmt(ev.StartsAt)}");
        sb.AppendLine($"DTEND:{Fmt(ev.EndsAt)}");
        sb.AppendLine($"SUMMARY:{Esc(ev.Title)}");
        var description = ev.Description;
        if (!string.IsNullOrWhiteSpace(baseUrl))
        {
            description = $"{description}\n\n{baseUrl.TrimEnd('/')}/events/{ev.Slug}";
        }
        sb.AppendLine($"DESCRIPTION:{Esc(description)}");
        if (!string.IsNullOrWhiteSpace(ev.Location))
        {
            sb.AppendLine($"LOCATION:{Esc(ev.Location)}");
        }
        sb.AppendLine($"STATUS:{(ev.Status == EventStatus.Cancelled ? "CANCELLED" : "CONFIRMED")}");
        sb.AppendLine("END:VEVENT");
        sb.AppendLine("END:VCALENDAR");
        return sb.ToString();
    }
}
