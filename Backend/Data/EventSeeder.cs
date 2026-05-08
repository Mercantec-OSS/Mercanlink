using Backend.Models;
using Backend.Services;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public static class EventSeeder
{
    public static async Task SeedDefaultsAsync(ApplicationDbContext db, EventsService eventsService, ILogger logger)
    {
        var defaults = new List<EventSeedSpec>
        {
            new()
            {
                Title = "Webinar: CEDCE – omkring Datacenter Management",
                Slug = "webinar-cedce-datacenter-management",
                Type = EventType.Talk,
                StartsAtUtc = new DateTime(2026, 5, 11, 13, 0, 0, DateTimeKind.Utc),
                EndsAtUtc = new DateTime(2026, 5, 11, 15, 0, 0, DateTimeKind.Utc),
                Location = "Online (webinar)",
                Description =
                    "Embark on a transformative journey into the heart of digital infrastructure. Discover the dynamic world of Datacenter management, maintenance and operations, where innovation meets opportunity.\n\n" +
                    "Guest speakers:\n" +
                    "- Roy Maxwell\n" +
                    "- Daniel Stölzle (Business Development Sustainability)\n" +
                    "- Ivan Hemmeler (Business Development Manager Spain & Portugal)",
                SpeakerName = "Roy Maxwell, Daniel Stölzle, Ivan Hemmeler",
                BannerImageUrl = "/CEDCEBanner.png",
            },
            new()
            {
                Title = "GF2 brætspilscafé – alle er velkomne",
                Slug = "gf2-braetspilscafe",
                Type = EventType.Other,
                StartsAtUtc = new DateTime(2026, 5, 18, 12, 30, 0, DateTimeKind.Utc),
                EndsAtUtc = new DateTime(2026, 5, 18, 15, 30, 0, DateTimeKind.Utc),
                Location = "Mercantec",
                Description = "GF2 afholder brætspilscafé, og alle er velkomne. Kom forbi og spil med!",
                BannerImageUrl = "/BoardgameBanner.png",
            },
            new()
            {
                Title = "Git Good At Git – GitHub workshop (H5)",
                Slug = "git-good-at-git-github-workshop-h5",
                Type = EventType.Workshop,
                StartsAtUtc = new DateTime(2026, 5, 20, 10, 0, 0, DateTimeKind.Utc),
                EndsAtUtc = new DateTime(2026, 5, 20, 12, 0, 0, DateTimeKind.Utc),
                Location = "Mercantec",
                Description = "GitHub workshop for alle med fokus på GitHub til lærepladssøgning og struktur.",
                Prerequisites = "Medbring gerne en bærbar (valgfrit) og en GitHub-konto.",
                BringOwnPc = true,
                BannerImageUrl = "/GitHubBanner.png",
            }
        };

        // Upsert + oprydning: behold kun ét event pr. seed-spec, og sørg for banner.
        var titles = defaults.Select(d => d.Title).ToList();
        var seedCandidates = await db.Events
            .Where(e => titles.Contains(e.Title) || defaults.Select(d => d.Slug).Contains(e.Slug))
            .OrderByDescending(e => e.UpdatedAt)
            .ToListAsync();

        var removed = 0;
        var created = 0;
        var updated = 0;

        foreach (var d in defaults)
        {
            var matches = seedCandidates
                .Where(e => e.Slug == d.Slug || e.Title == d.Title)
                .OrderByDescending(e => e.UpdatedAt)
                .ToList();

            var keep = matches.FirstOrDefault();
            if (keep != null)
            {
                // Slet dubletter
                foreach (var extra in matches.Skip(1))
                {
                    db.Events.Remove(extra);
                    removed++;
                }

                // Opdater den "rigtige" række (inkl. banner)
                keep.Title = d.Title;
                keep.Slug = d.Slug;
                keep.Description = d.Description;
                keep.Type = d.Type;
                keep.Status = EventStatus.Published;
                keep.StartsAt = d.StartsAtUtc;
                keep.EndsAt = d.EndsAtUtc;
                keep.Location = d.Location;
                keep.BannerImageUrl = d.BannerImageUrl;
                keep.SpeakerName = d.SpeakerName;
                keep.Prerequisites = d.Prerequisites;
                keep.TeamSize = d.TeamSize;
                keep.BringOwnPc = d.BringOwnPc;
                keep.UpdatedAt = DateTime.UtcNow;
                updated++;
            }
            else
            {
                // Hvis slug allerede findes på et andet event, gør slug unik på en kontrolleret måde
                var desiredSlug = d.Slug;
                if (await db.Events.AnyAsync(e => e.Slug == desiredSlug))
                {
                    desiredSlug = await eventsService.EnsureUniqueSlugAsync(desiredSlug);
                }

                var ev = new Event
                {
                    Title = d.Title,
                    Slug = desiredSlug,
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
                created++;
            }
        }

        if (removed > 0 || created > 0 || updated > 0)
        {
            await db.SaveChangesAsync();
        }

        logger.LogInformation(
            "Event seed: created={Created}, updated={Updated}, removedDuplicates={Removed}.",
            created,
            updated,
            removed
        );
    }

    private sealed class EventSeedSpec
    {
        public string Title { get; init; } = string.Empty;
        public string Slug { get; init; } = string.Empty;
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

