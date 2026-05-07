using Backend.Data;
using Backend.Models;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace Backend.Discord;

/// <summary>
/// Håndterer Discord-annoncering, redigering, aflysning og scheduled events for platformens events.
/// </summary>
public class EventDiscordAnnouncer
{
    private readonly DiscordBotService _bot;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<EventDiscordAnnouncer> _logger;
    private readonly ulong _eventChannelId;

    public EventDiscordAnnouncer(
        DiscordBotService bot,
        ApplicationDbContext context,
        IConfiguration configuration,
        ILogger<EventDiscordAnnouncer> logger)
    {
        _bot = bot;
        _context = context;
        _logger = logger;
        var raw = Environment.GetEnvironmentVariable("DISCORD_EVENTCHANNELID")
            ?? configuration["Discord:EventChannelId"]
            ?? "0";
        ulong.TryParse(raw, out _eventChannelId);
    }

    public bool IsConfigured => _eventChannelId != 0;

    public async Task PublishAsync(Event ev)
    {
        if (!_bot.IsReady)
        {
            _logger.LogWarning("Discord bot er ikke klar; springer announcement for event {EventId} over.", ev.Id);
            return;
        }

        if (!IsConfigured)
        {
            _logger.LogWarning("Discord:EventChannelId er ikke sat; springer announcement for event {EventId} over.", ev.Id);
            return;
        }

        var channel = _bot.Client.GetChannel(_eventChannelId) as IMessageChannel;
        if (channel == null)
        {
            _logger.LogWarning("Kunne ikke finde Discord event-kanal {ChannelId}", _eventChannelId);
            return;
        }

        var embed = BuildEmbed(ev);
        var components = BuildComponents(ev);

        IUserMessage? message = null;
        if (ev.DiscordAnnouncementMessageId.HasValue && ev.DiscordChannelId.HasValue)
        {
            try
            {
                var existingChannel = _bot.Client.GetChannel(ev.DiscordChannelId.Value) as IMessageChannel;
                if (existingChannel != null)
                {
                    var existing = await existingChannel.GetMessageAsync(ev.DiscordAnnouncementMessageId.Value) as IUserMessage;
                    if (existing != null)
                    {
                        await existing.ModifyAsync(m =>
                        {
                            m.Embed = embed;
                            m.Components = components;
                        });
                        message = existing;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Kunne ikke redigere eksisterende Discord-besked for event {EventId}; sender ny.", ev.Id);
            }
        }

        if (message == null)
        {
            message = await channel.SendMessageAsync(embed: embed, components: components);
        }

        var guild = _bot.Client.GetGuild(_bot.GuildId);
        ulong? scheduledEventId = ev.DiscordScheduledEventId;
        if (guild != null)
        {
            try
            {
                if (scheduledEventId.HasValue)
                {
                    var existingEvent = await guild.GetEventAsync(scheduledEventId.Value);
                    if (existingEvent != null)
                    {
                        await existingEvent.ModifyAsync(props =>
                        {
                            props.Name = ev.Title;
                            props.Description = TruncateForDiscord(ev.Description, 1000);
                            props.StartTime = new DateTimeOffset(EnsureUtc(ev.StartsAt));
                            props.EndTime = new DateTimeOffset(EnsureUtc(ev.EndsAt));
                            props.Location = ev.Location;
                        });
                    }
                    else
                    {
                        scheduledEventId = null;
                    }
                }

                if (!scheduledEventId.HasValue)
                {
                    var created = await guild.CreateEventAsync(
                        name: ev.Title,
                        startTime: new DateTimeOffset(EnsureUtc(ev.StartsAt)),
                        type: GuildScheduledEventType.External,
                        privacyLevel: GuildScheduledEventPrivacyLevel.Private,
                        description: TruncateForDiscord(ev.Description, 1000),
                        endTime: new DateTimeOffset(EnsureUtc(ev.EndsAt)),
                        location: string.IsNullOrWhiteSpace(ev.Location) ? "Mercantec" : ev.Location
                    );
                    scheduledEventId = created.Id;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Kunne ikke oprette/opdatere Discord Scheduled Event for {EventId}", ev.Id);
            }
        }

        var tracked = await _context.Events.FirstOrDefaultAsync(e => e.Id == ev.Id);
        if (tracked != null)
        {
            tracked.DiscordChannelId = channel.Id;
            tracked.DiscordAnnouncementMessageId = message.Id;
            tracked.DiscordScheduledEventId = scheduledEventId;
            tracked.AnnouncementSentAt = DateTime.UtcNow;
            tracked.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateAnnouncementAsync(Event ev)
    {
        if (!_bot.IsReady || !ev.DiscordAnnouncementMessageId.HasValue || !ev.DiscordChannelId.HasValue)
        {
            return;
        }

        var channel = _bot.Client.GetChannel(ev.DiscordChannelId.Value) as IMessageChannel;
        if (channel == null) return;

        try
        {
            var msg = await channel.GetMessageAsync(ev.DiscordAnnouncementMessageId.Value) as IUserMessage;
            if (msg != null)
            {
                await msg.ModifyAsync(m =>
                {
                    m.Embed = BuildEmbed(ev);
                    m.Components = BuildComponents(ev);
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Kunne ikke opdatere Discord-announcement for event {EventId}", ev.Id);
        }

        var guild = _bot.Client.GetGuild(_bot.GuildId);
        if (guild != null && ev.DiscordScheduledEventId.HasValue)
        {
            try
            {
                var existingEvent = await guild.GetEventAsync(ev.DiscordScheduledEventId.Value);
                if (existingEvent != null)
                {
                    await existingEvent.ModifyAsync(props =>
                    {
                        props.Name = ev.Title;
                        props.Description = TruncateForDiscord(ev.Description, 1000);
                        props.StartTime = new DateTimeOffset(EnsureUtc(ev.StartsAt));
                        props.EndTime = new DateTimeOffset(EnsureUtc(ev.EndsAt));
                        props.Location = ev.Location;
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Kunne ikke opdatere Discord Scheduled Event for {EventId}", ev.Id);
            }
        }
    }

    public async Task CancelAsync(Event ev)
    {
        if (!_bot.IsReady) return;

        if (ev.DiscordChannelId.HasValue && ev.DiscordAnnouncementMessageId.HasValue)
        {
            var channel = _bot.Client.GetChannel(ev.DiscordChannelId.Value) as IMessageChannel;
            if (channel != null)
            {
                try
                {
                    var msg = await channel.GetMessageAsync(ev.DiscordAnnouncementMessageId.Value) as IUserMessage;
                    if (msg != null)
                    {
                        await msg.ModifyAsync(m =>
                        {
                            m.Embed = BuildEmbed(ev, cancelled: true);
                            m.Components = new ComponentBuilder().Build();
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Kunne ikke markere Discord-besked som aflyst for {EventId}", ev.Id);
                }
            }
        }

        if (ev.DiscordScheduledEventId.HasValue)
        {
            var guild = _bot.Client.GetGuild(_bot.GuildId);
            if (guild != null)
            {
                try
                {
                    var existing = await guild.GetEventAsync(ev.DiscordScheduledEventId.Value);
                    if (existing != null)
                    {
                        await existing.DeleteAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Kunne ikke slette Discord Scheduled Event for {EventId}", ev.Id);
                }
            }
        }
    }

    public async Task SendReminderAsync(Event ev, string headline)
    {
        if (!_bot.IsReady || !IsConfigured) return;

        var channel = _bot.Client.GetChannel(_eventChannelId) as IMessageChannel;
        if (channel == null) return;

        var embed = new EmbedBuilder()
            .WithTitle($"⏰ {headline}: {ev.Title}")
            .WithDescription($"Eventet starter <t:{new DateTimeOffset(EnsureUtc(ev.StartsAt)).ToUnixTimeSeconds()}:R>.")
            .AddField("Hvornår", $"<t:{new DateTimeOffset(EnsureUtc(ev.StartsAt)).ToUnixTimeSeconds()}:F>", inline: true)
            .AddField("Hvor", string.IsNullOrWhiteSpace(ev.Location) ? "Mercantec" : ev.Location, inline: true)
            .WithColor(new Color(0xf59e0b))
            .WithCurrentTimestamp()
            .Build();

        try
        {
            await channel.SendMessageAsync(embed: embed);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Kunne ikke sende reminder for event {EventId}", ev.Id);
        }
    }

    public static string GetRegisterButtonId(string eventId) => $"event:{eventId}:register";
    public static string GetUnregisterButtonId(string eventId) => $"event:{eventId}:unregister";

    private static Embed BuildEmbed(Event ev, bool cancelled = false)
    {
        var color = cancelled
            ? new Color(0xb91c1c)
            : ev.Type switch
            {
                EventType.Lan => new Color(0x4f46e5),
                EventType.Workshop => new Color(0x059669),
                EventType.Talk => new Color(0xdb2777),
                EventType.Hackathon => new Color(0xf59e0b),
                _ => new Color(0x447ef2)
            };

        var startUnix = new DateTimeOffset(EnsureUtc(ev.StartsAt)).ToUnixTimeSeconds();
        var endUnix = new DateTimeOffset(EnsureUtc(ev.EndsAt)).ToUnixTimeSeconds();

        var typeBadge = ev.Type switch
        {
            EventType.Lan => "🎮 LAN",
            EventType.Workshop => "🛠️ Workshop",
            EventType.Talk => "🎤 Foredrag",
            EventType.Hackathon => "💡 Hackathon",
            _ => "📌 Event"
        };

        var titlePrefix = cancelled ? "❌ AFLYST: " : string.Empty;

        var builder = new EmbedBuilder()
            .WithTitle($"{titlePrefix}{typeBadge} · {ev.Title}")
            .WithDescription(TruncateForDiscord(ev.Description, 2000))
            .AddField("Hvornår", $"<t:{startUnix}:F> – <t:{endUnix}:t>", inline: false)
            .AddField("Hvor", string.IsNullOrWhiteSpace(ev.Location) ? "Mercantec" : ev.Location, inline: true)
            .WithColor(color)
            .WithCurrentTimestamp();

        if (ev.Capacity.HasValue)
        {
            builder.AddField("Pladser", ev.Capacity.Value.ToString(), inline: true);
        }

        if (ev.RegistrationDeadline.HasValue)
        {
            var deadlineUnix = new DateTimeOffset(EnsureUtc(ev.RegistrationDeadline.Value)).ToUnixTimeSeconds();
            builder.AddField("Tilmeldingsfrist", $"<t:{deadlineUnix}:F>", inline: true);
        }

        if (ev.BringOwnPc == true && ev.Type == EventType.Lan)
        {
            builder.AddField("OBS", "Medbring egen PC", inline: false);
        }

        if (!string.IsNullOrWhiteSpace(ev.SpeakerName))
        {
            builder.AddField("Oplægsholder", ev.SpeakerName, inline: true);
        }

        if (!string.IsNullOrWhiteSpace(ev.BannerImageUrl))
        {
            builder.WithImageUrl(ev.BannerImageUrl);
        }

        return builder.Build();
    }

    private static MessageComponent BuildComponents(Event ev)
    {
        var builder = new ComponentBuilder()
            .WithButton("Tilmeld dig", GetRegisterButtonId(ev.Id), ButtonStyle.Success, new Emoji("✅"))
            .WithButton("Frameld", GetUnregisterButtonId(ev.Id), ButtonStyle.Secondary, new Emoji("✖️"));
        return builder.Build();
    }

    private static DateTime EnsureUtc(DateTime dt)
    {
        return dt.Kind switch
        {
            DateTimeKind.Utc => dt,
            DateTimeKind.Local => dt.ToUniversalTime(),
            _ => DateTime.SpecifyKind(dt, DateTimeKind.Utc)
        };
    }

    private static string TruncateForDiscord(string? text, int max)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;
        return text.Length <= max ? text : text.Substring(0, max - 1) + "…";
    }
}
