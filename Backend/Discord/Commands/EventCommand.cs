using Backend.Data;
using Backend.Models;
using Backend.Services;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Backend.DiscordBot.Commands;

public partial class Commands
{
    /// <summary>
    /// !event liste | tilmeld &lt;slug&gt; | frameld &lt;slug&gt;
    /// </summary>
    private static async Task EventCommand(SocketMessage message, DiscordSocketClient client)
    {
        if (_serviceProvider == null)
        {
            await message.Channel.SendMessageAsync("Fejl: Service provider er ikke konfigureret.");
            return;
        }

        var content = message.Content.Trim();
        var parts = content.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);

        var sub = parts.Length > 1 ? parts[1].ToLowerInvariant() : "liste";
        var arg = parts.Length > 2 ? parts[2].Trim() : null;

        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var eventsService = scope.ServiceProvider.GetRequiredService<EventsService>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Commands>>();

        try
        {
            switch (sub)
            {
                case "liste":
                case "list":
                    await SendEventListAsync(message, eventsService);
                    return;
                case "tilmeld":
                case "register":
                case "join":
                    if (string.IsNullOrWhiteSpace(arg))
                    {
                        await message.Channel.SendMessageAsync("Angiv hvilket event du vil tilmelde dig: `!event tilmeld <slug>`");
                        return;
                    }
                    await HandleEventRegisterAsync(message, dbContext, eventsService, arg);
                    return;
                case "frameld":
                case "unregister":
                case "leave":
                    if (string.IsNullOrWhiteSpace(arg))
                    {
                        await message.Channel.SendMessageAsync("Angiv hvilket event du vil framelde dig: `!event frameld <slug>`");
                        return;
                    }
                    await HandleEventUnregisterAsync(message, dbContext, eventsService, arg);
                    return;
                default:
                    await message.Channel.SendMessageAsync(
                        "Brug: `!event liste`, `!event tilmeld <slug>` eller `!event frameld <slug>`."
                    );
                    return;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fejl ved !event kommando");
            await message.Channel.SendMessageAsync("Der opstod en fejl ved håndtering af event-kommandoen.");
        }
    }

    private static async Task SendEventListAsync(SocketMessage message, EventsService eventsService)
    {
        var events = await eventsService.ListAsync(upcoming: true, type: null, includeUnpublished: false);
        var publishedEvents = events.Where(e => e.Status == EventStatus.Published).Take(10).ToList();

        if (publishedEvents.Count == 0)
        {
            await message.Channel.SendMessageAsync("Der er ingen kommende events lige nu.");
            return;
        }

        var embed = new EmbedBuilder()
            .WithTitle("📅 Kommende events")
            .WithColor(new Color(0x447ef2))
            .WithCurrentTimestamp();

        foreach (var ev in publishedEvents)
        {
            var startUnix = new DateTimeOffset(DateTime.SpecifyKind(ev.StartsAt, DateTimeKind.Utc)).ToUnixTimeSeconds();
            embed.AddField(
                $"{TypeBadge(ev.Type)} {ev.Title}",
                $"<t:{startUnix}:F>\n📍 {ev.Location}\n`!event tilmeld {ev.Slug}`"
            );
        }

        await message.Channel.SendMessageAsync(embed: embed.Build());
    }

    private static async Task HandleEventRegisterAsync(SocketMessage message, ApplicationDbContext dbContext, EventsService eventsService, string slug)
    {
        var ev = await eventsService.GetBySlugAsync(slug);
        if (ev == null || ev.Status != EventStatus.Published)
        {
            await message.Channel.SendMessageAsync($"Kunne ikke finde et åbent event med slug `{slug}`.");
            return;
        }

        var user = await ResolveUserFromDiscordAsync(dbContext, message.Author.Id.ToString());
        if (user == null)
        {
            await message.Channel.SendMessageAsync(
                "Du er ikke registreret. Brug `!register` for at oprette dig, og log derefter ind på hub.mercantec.tech for at bekræfte din edu-mail."
            );
            return;
        }

        var email = user.WebsiteUser?.Email ?? string.Empty;
        if (!eventsService.IsValidEduEmail(email))
        {
            await message.Channel.SendMessageAsync(
                $"Du skal have en gyldig Mercantec edu-mail på din profil før du kan tilmelde dig via Discord.\n" +
                $"Åbn https://hub.mercantec.tech/events/{ev.Slug} og tilmeld dig der."
            );
            return;
        }

        var displayName = AuthenticatedUserService.ResolveDisplayName(user);
        var outcome = await eventsService.RegisterAsync(ev, user, displayName, email, EventRegistrationSource.Discord);

        var reply = outcome.Result switch
        {
            EventRegistrationResult.Success => $"✅ Du er tilmeldt **{ev.Title}**.",
            EventRegistrationResult.AlreadyRegistered => "Du er allerede tilmeldt dette event.",
            EventRegistrationResult.Full => "❌ Eventet er fyldt op.",
            EventRegistrationResult.DeadlinePassed => "❌ Tilmeldingsfristen er overskredet.",
            EventRegistrationResult.NotPublished => "❌ Eventet er ikke åbent for tilmelding.",
            EventRegistrationResult.InvalidEmail => "❌ Din edu-mail kunne ikke verificeres.",
            _ => "Ukendt fejl ved tilmelding."
        };
        await message.Channel.SendMessageAsync(reply);
    }

    private static async Task HandleEventUnregisterAsync(SocketMessage message, ApplicationDbContext dbContext, EventsService eventsService, string slug)
    {
        var ev = await eventsService.GetBySlugAsync(slug);
        if (ev == null)
        {
            await message.Channel.SendMessageAsync($"Kunne ikke finde et event med slug `{slug}`.");
            return;
        }

        var user = await ResolveUserFromDiscordAsync(dbContext, message.Author.Id.ToString());
        if (user == null)
        {
            await message.Channel.SendMessageAsync("Du er ikke registreret.");
            return;
        }

        var ok = await eventsService.UnregisterAsync(ev.Id, user.Id);
        await message.Channel.SendMessageAsync(ok
            ? $"✅ Du er frameldt **{ev.Title}**."
            : "Du var ikke tilmeldt dette event.");
    }

    public static async Task<User?> ResolveUserFromDiscordAsync(ApplicationDbContext dbContext, string discordId)
    {
        return await dbContext.Users
            .Include(u => u.DiscordUser)
            .Include(u => u.WebsiteUser)
            .FirstOrDefaultAsync(u => u.DiscordUser != null && u.DiscordUser.DiscordId == discordId);
    }

    private static string TypeBadge(EventType type) => type switch
    {
        EventType.Lan => "🎮",
        EventType.Workshop => "🛠️",
        EventType.Talk => "🎤",
        EventType.Hackathon => "💡",
        _ => "📌"
    };
}
