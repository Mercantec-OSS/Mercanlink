using Backend.Models;
using Backend.Services;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public static class EventSeeder
{
    public static async Task SeedDefaultsAsync(ApplicationDbContext db, EventsService eventsService, ILogger logger)
    {
        // Idempotent: opret kun hvis de ikke allerede findes.
        var existingSlugs = await db.Events.AsNoTracking().Select(e => e.Slug).ToListAsync();
        var existing = new HashSet<string>(existingSlugs);

        var defaults = new List<EventSeedSpec>
        {
            new()
            {
                Title = "Webinar: CEDCE – omkring Datacenter Management",
                Type = EventType.Talk,
                StartsAtUtc = new DateTime(2026, 5, 11, 17, 0, 0, DateTimeKind.Utc),
                EndsAtUtc = new DateTime(2026, 5, 11, 19, 0, 0, DateTimeKind.Utc),
                Location = "Online (webinar)",
                Description =
                    "Embark on a transformative journey into the heart of digital infrastructure. Discover the dynamic world of Datacenter management, maintenance and operations, where innovation meets opportunity.\n\n" +
                    "Guest speakers:\n" +
                    "- Roy Maxwell\n" +
                    "- Daniel Stölzle (Business Development Sustainability)\n" +
                    "- Ivan Hemmeler (Business Development Manager Spain & Portugal)",
                SpeakerName = "Roy Maxwell, Daniel Stölzle, Ivan Hemmeler",
            },
            new()
            {
                Title = "GF2 brætspilscafé – alle er velkomne",
                Type = EventType.Other,
                StartsAtUtc = new DateTime(2026, 5, 18, 15, 0, 0, DateTimeKind.Utc),
                EndsAtUtc = new DateTime(2026, 5, 18, 18, 0, 0, DateTimeKind.Utc),
                Location = "Mercantec",
                Description = "GF2 afholder brætspilscafé, og alle er velkomne. Kom forbi og spil med!",
            },
            new()
            {
                Title = "Git Good At Git – GitHub workshop (H5)",
                Type = EventType.Workshop,
                StartsAtUtc = new DateTime(2026, 5, 20, 14, 0, 0, DateTimeKind.Utc),
                EndsAtUtc = new DateTime(2026, 5, 20, 16, 0, 0, DateTimeKind.Utc),
                Location = "Mercantec",
                Description = "GitHub workshop for alle med fokus på GitHub til lærepladssøgning og struktur.",
                Prerequisites = "Medbring gerne en bærbar (valgfrit) og en GitHub-konto.",
                BringOwnPc = true
            }
        };

        var created = 0;
        foreach (var d in defaults)
        {
            var slugBase = eventsService.GenerateSlug(d.Title);
            var slug = await eventsService.EnsureUniqueSlugAsync(slugBase);
            if (existing.Contains(slug))
            {
                continue;
            }

            var ev = new Event
            {
                Title = d.Title,
                Slug = slug,
                Description = d.Description,
                Type = d.Type,
                Status = EventStatus.Published,
                StartsAt = d.StartsAtUtc,
                EndsAt = d.EndsAtUtc,
                Location = d.Location,
                BannerImageUrl = d.BannerImageUrl,
                SpeakerName = d.SpeakerName,
                Prerequisites = d.Prerequisites,
                TeamSize = d.TeamSize,
                BringOwnPc = d.BringOwnPc,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            db.Events.Add(ev);
            existing.Add(slug);
            created++;
        }

        if (created > 0)
        {
            await db.SaveChangesAsync();
            logger.LogInformation("Seedede {Count} default events.", created);
        }
        else
        {
            logger.LogInformation("Ingen default events seedet (allerede til stede).");
        }
    }

    private sealed class EventSeedSpec
    {
        public string Title { get; init; } = string.Empty;
        public EventType Type { get; init; } = EventType.Other;
        public DateTime StartsAtUtc { get; init; }
        public DateTime EndsAtUtc { get; init; }
        public string Location { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string? BannerImageUrl { get; init; }
        public string? SpeakerName { get; init; }
        public string? Prerequisites { get; init; }
        public int? TeamSize { get; init; }
        public bool? BringOwnPc { get; init; }
    }
}

