using Backend.Discord;
using Backend.DiscordBot.Commands;
using Backend.DiscordServices.Services;
using Backend.Models.DTOs;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using Backend.Discord.Enums;
using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

public class DiscordBotService
{
    private readonly DiscordSocketClient _client;
    private readonly string _token;
    private readonly string _prefix = "!"; // Prefix for kommandoer
    private readonly ulong _roleSelectionChannelId = 1358696771596980326;
    private readonly ulong _modChannelId;
    private readonly ulong _knowledgeCenterChannelId;
    private readonly ulong _modRoleId;
    private readonly Dictionary<string, ulong> _roleMap = new() { { "👍", 1353709131500093532 } };
    private readonly ulong _guildId = 1351185531836436541;
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<ulong, DateTime> _voiceUsers = new Dictionary<ulong, DateTime>();
    private DateTime? _lastGatewayActivityUtc;
    private DateTime? _lastReadyUtc;
    private DateTime? _lastDisconnectUtc;
    private string? _lastDisconnectReason;
    private bool _isReady;
    private bool _isConnected;

    public DiscordBotService(IConfiguration config, IServiceProvider serviceProvider)
    {
        _token = Environment.GetEnvironmentVariable("DISCORD_TOKEN") ?? config["Discord:Token"];
        _modChannelId = Convert.ToUInt64(Environment.GetEnvironmentVariable("DISCORD_MODCHANNELID") ?? config["Discord:ModChannelId"]);
        _knowledgeCenterChannelId = Convert.ToUInt64(Environment.GetEnvironmentVariable("DISCORD_KNOWLEDGECENTERCHANNELID") ?? config["Discord:KnowledgeCenterChannelId"]);
        _modRoleId = Convert.ToUInt64(Environment.GetEnvironmentVariable("DISCORD_MODROLEID") ?? config["Discord:ModRoleId"]);

        // Debug: Tjek om værdierne er blevet hentet korrekt
        Console.WriteLine("=== DiscordBotService Konfiguration ===");
        Console.WriteLine($"Token: {(string.IsNullOrEmpty(_token) ? "MANGELENDE" : "OK")}");
        Console.WriteLine($"Mod Channel ID: {_modChannelId}");
        Console.WriteLine($"Knowledge Center Channel ID: {_knowledgeCenterChannelId}");
        Console.WriteLine($"Mod Role ID: {_modRoleId}");
        Console.WriteLine($"Role Selection Channel ID: {_roleSelectionChannelId}");
        Console.WriteLine($"Guild ID: {_guildId}");
        Console.WriteLine("========================================");
        _client = new DiscordSocketClient(
            new DiscordSocketConfig
            {
                GatewayIntents =
                    GatewayIntents.AllUnprivileged
                    | GatewayIntents.MessageContent
                    | GatewayIntents.GuildMembers
            }
        );

        _serviceProvider = serviceProvider;

        // Sæt service provider i Commands klassen
        Commands.SetServiceProvider(serviceProvider);

        // Sæt denne instans i Commands klassen
        Commands.SetDiscordBotService(this);
    }

    public async Task StartAsync()
    {
        _isReady = false;
        _isConnected = false;

        _client.Log += LogAsync;
        _client.Connected += HandleConnectedAsync;
        _client.Disconnected += HandleDisconnectedAsync;
        _client.LatencyUpdated += HandleLatencyUpdatedAsync;
        _client.Ready += async () =>
        {
            _isReady = true;
            _lastReadyUtc = DateTime.UtcNow;
            UpdateGatewayActivity();
            Console.WriteLine($"Bot er forbundet til {_client.Guilds.Count} servere!");
            foreach (var guild in _client.Guilds)
            {
                Console.WriteLine($" - {guild.Name} (ID: {guild.Id})");
            }

            var channel = _client.GetChannel(_roleSelectionChannelId) as SocketTextChannel;

            if (channel != null)
            {
                await SendRoleMessageAsync(channel);
            }

            return;
        };

        // Tilføj message handler
        _client.MessageReceived += HandleCommandAsync;

        // Tilføj XP event handlers
        _client.MessageReceived += HandleMessageXpAsync;
        _client.ReactionAdded += HandleReactionXpAsync;
        _client.UserVoiceStateUpdated += HandleVoiceXpAsync;

        // Tilføj reaction handlers
        _client.ReactionAdded += ReactionAddedAsync;
        _client.ReactionRemoved += ReactionRemovedAsync;

        // Registrer bruger
        _client.UserJoined += HandleRegisterAsync;

        await _client.LoginAsync(TokenType.Bot, _token);
        await _client.StartAsync();
    }

    public DiscordHealthSnapshot GetHealthSnapshot()
    {
        return new DiscordHealthSnapshot
        {
            IsConnected = _isConnected,
            IsReady = _isReady,
            LastGatewayActivityUtc = _lastGatewayActivityUtc,
            LastReadyUtc = _lastReadyUtc,
            LastDisconnectUtc = _lastDisconnectUtc,
            LastDisconnectReason = _lastDisconnectReason,
            ConnectionState = _client.ConnectionState.ToString(),
            LoginState = _client.LoginState.ToString()
        };
    }

    private async Task HandleCommandAsync(SocketMessage messageParam)
    {
        UpdateGatewayActivity();

        // Ignorer beskeder fra bots
        if (messageParam is not SocketUserMessage message || message.Author.IsBot)
            return;

        // Tjek om beskeden starter med prefixet
        int argPos = 0;
        if (!message.HasStringPrefix(_prefix, ref argPos))
            return;

        // Få kommandoen og argumenterne
        string commandText = message.Content.Substring(argPos);
        string[] args = commandText.Split(' ');
        string command = args[0].ToLower();

        // Håndter kommandoen via Commands klassen
        if (Commands.CommandExists(command))
        {
            await Commands.ExecuteCommand(command, message, _client);
        }
        else
        {
            await message.Channel.SendMessageAsync(
                $"Ukendt kommando. Skriv `{_prefix}help` for at se tilgængelige kommandoer."
            );
        }
    }

    private async Task SendRoleMessageAsync(SocketTextChannel channel)
    {
        var messages = await channel.GetMessagesAsync(50).FlattenAsync();
        var existing = messages.FirstOrDefault(m =>
            m.Author.Id == _client.CurrentUser.Id
            && m.Content.Contains("Get roles corresponding to reactions")
        );

        if (existing != null)
            return;

        var newMessage = await channel.SendMessageAsync("Get roles corresponding to reactions");
        await newMessage.AddReactionAsync(new Emoji("👍"));
    }

    private async Task ReactionAddedAsync(
        Cacheable<IUserMessage, ulong> cachedMessage,
        Cacheable<IMessageChannel, ulong> cachedChannel,
        SocketReaction reaction
    )
    {
        UpdateGatewayActivity();
        var emojiName = reaction.Emote.Name;

        if (reaction.User.Value.IsBot)
            return;

        if (reaction.Channel.Id == _modChannelId) { await KnowledgeCenterPostReaction(reaction, cachedMessage); }

        if (_roleMap.TryGetValue(reaction.Emote.Name, out ulong roleId))
        {
            var guild = _client.GetGuild(_guildId);
            var user = guild.GetUser(reaction.UserId);
            var role = guild.GetRole(roleId);

            if (role != null && user != null)
            {
                await user.AddRoleAsync(role);
            }
        }
    }

    private async Task ReactionRemovedAsync(
        Cacheable<IUserMessage, ulong> cachedMessage,
        Cacheable<IMessageChannel, ulong> cachedChannel,
        SocketReaction reaction
    )
    {
        UpdateGatewayActivity();

        if (reaction.User.Value.IsBot)
            return;

        if (_roleMap.TryGetValue(reaction.Emote.Name, out ulong roleId))
        {
            var guild = _client.GetGuild(_guildId);
            var user = guild.GetUser(reaction.UserId);
            var role = guild.GetRole(roleId);

            if (role != null && user != null)
            {
                await user.RemoveRoleAsync(role);
            }
        }
    }

    // XP for beskeder
    private async Task HandleMessageXpAsync(SocketMessage message)
    {
        UpdateGatewayActivity();

        // Ignorer beskeder fra bots
        if (message.Author.IsBot)
            return;

        // Brug en scope for at få XPService
        using (var scope = _serviceProvider.CreateScope())
        {
            var xpService = scope.ServiceProvider.GetRequiredService<XPService>();

            // Tjek og tildel daglig login bonus først
            await xpService.CheckAndAwardDailyLoginAsync(message.Author.Id.ToString());

            // Derefter giv XP for beskeden
            await xpService.AddXPAsync(message.Author.Id.ToString(), XpActivityType.Message);
        }
    }

    // XP for reaktioner
    private async Task HandleReactionXpAsync(
        Cacheable<IUserMessage, ulong> cachedMessage,
        Cacheable<IMessageChannel, ulong> cachedChannel,
        SocketReaction reaction
    )
    {
        UpdateGatewayActivity();

        // Ignorer reaktioner fra bots
        if (reaction.User.IsSpecified && reaction.User.Value.IsBot)
            return;

        using (var scope = _serviceProvider.CreateScope())
        {
            var xpService = scope.ServiceProvider.GetRequiredService<XPService>();
            if (reaction.UserId.ToString() != null)
            {
                // Tjek og tildel daglig login bonus først
                await xpService.CheckAndAwardDailyLoginAsync(reaction.UserId.ToString());

                // Derefter giv XP for reaktionen
                await xpService.AddXPAsync(reaction.UserId.ToString(), XpActivityType.Reaction);
            }
        }
    }

    // XP for voice chat
    private async Task HandleVoiceXpAsync(
        SocketUser user,
        SocketVoiceState oldState,
        SocketVoiceState newState
    )
    {
        UpdateGatewayActivity();

        // Ignorer bots
        if (user.IsBot)
            return;

        // Bruger tilslutter voice
        if (oldState.VoiceChannel == null && newState.VoiceChannel != null)
        {
            _voiceUsers[user.Id] = DateTime.UtcNow;

            // Tildel daglig login bonus når brugeren tilslutter voice
            using (var scope = _serviceProvider.CreateScope())
            {
                var xpService = scope.ServiceProvider.GetRequiredService<XPService>();
                await xpService.CheckAndAwardDailyLoginAsync(user.Id.ToString());
            }
        }
        // Bruger forlader voice
        else if (oldState.VoiceChannel != null && newState.VoiceChannel == null)
        {
            if (_voiceUsers.TryGetValue(user.Id, out DateTime joinTime))
            {
                TimeSpan timeInVoice = DateTime.UtcNow - joinTime;
                int minutesInVoice = (int)timeInVoice.TotalMinutes;

                if (minutesInVoice > 0)
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var xpService = scope.ServiceProvider.GetRequiredService<XPService>();

                        // Giv XP for hvert minut i voice
                        for (int i = 0; i < minutesInVoice; i++)
                        {
                            await xpService.AddXPAsync(
                                user.Id.ToString(),
                                XpActivityType.VoiceMinute
                            );
                        }
                    }
                }

                _voiceUsers.Remove(user.Id);
            }
        }
    }

    public async Task HandleRegisterAsync(SocketGuildUser guildUser)
    {
        UpdateGatewayActivity();

        if (_serviceProvider == null)
        {
            await guildUser.SendMessageAsync("Fejl: Service provider er ikke konfigureret.");
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<UserService>();
        var xpService = scope.ServiceProvider.GetRequiredService<XPService>();

        try
        {
            var user = await userService.CreateDiscordUserAsync(guildUser);
            bool isNewUser = user.CreatedAt > DateTime.UtcNow.AddMinutes(-1);

            var embed = new EmbedBuilder()
                .WithTitle(
                    "Velkommen til MercanLink!"
                )
                .WithDescription(
                    "Vær venlig og sæt dig ind i regelsettet og brugen af Discord Serveren.\n\nNeden for er nogle trin som kan hjælpe dig med at komme igang."
                )
                .WithColor(new Color(0x447ef2))
                .WithThumbnailUrl(guildUser.GetAvatarUrl() ?? guildUser.GetDefaultAvatarUrl())
                .WithCurrentTimestamp();

            // Tilføj altid regler og roller, uanset om brugeren er ny eller ej
            embed.AddField(
                "Læs Regelsættet",
                "I kanalen #Regler under Informations-kategorien finder du det nyeste og mest opdaterede regelsæt. Venligst læs dette og følg med i tilfælde af opdateringer hertil."
            );
            embed.AddField(
                "Vælg Roller",
                "I kanalen #Roller finder du en række reaktionsbeskeder, som du kan bruge til at vælge de roller, du ønsker. Det kan være ting som hvilken uddannelse du har, hvilke produkter/områder du interesserer dig for."
            );

            if (isNewUser)
            {
                await xpService.AddXPAsync(guildUser.Id.ToString(), XpActivityType.DailyLogin);
                embed.AddField(
                    "Mange tak!",
                    "Mange tak fordi du joinede MercanLink! Din Discord-konto er nu registreret i vores system. Du kan nu optjene XP og stige i level!"
                );
            }

            await guildUser.SendMessageAsync(embed: embed.Build());
        }
        catch (Exception ex)
        {
            await guildUser.SendMessageAsync(
                $"Der opstod en fejl under registrering: {ex.Message}"
            );
        }
    }

    private Task LogAsync(LogMessage log)
    {
        UpdateGatewayActivity();
        Console.WriteLine(log);
        return Task.CompletedTask;
    }

    public async Task SendLevelUpMessage(string discordId, int newLevel)
    {
        var user = _client.GetUser(ulong.Parse(discordId));
        if (user == null)
            return;
        try
        {
            await user.SendMessageAsync($"Tillykke! Du er steget til level {newLevel}! 🎉");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Kunne ikke sende level-up besked til {discordId}: {ex.Message}");
        }
    }

    /// <summary>
    /// Sender verification kode til Discord bruger
    /// </summary>
    /// <returns>True hvis beskeden blev sendt succesfuldt</returns>
    public async Task<bool> SendVerificationCodeAsync(string discordId, string verificationCode)
    {
        try
        {
            if (!ulong.TryParse(discordId, out var userIdUlong))
            {
                Console.WriteLine($"Ugyldig Discord ID format: {discordId}");
                return false;
            }
            var user = _client.GetUser(userIdUlong);
            if (user == null)
            {
                Console.WriteLine($"Kunne ikke finde Discord bruger: {discordId}");
                return false;
            }
            var message =
                $"🔐 **Mercantec-Space Verification**\n\n"
                + $"Din verification kode er: **{verificationCode}**\n\n"
                + $"Indtast denne kode på websiden for at linke din Discord konto.\n"
                + $"Koden udløber om 15 minutter.\n\n"
                + $"Hvis du ikke har anmodet om denne kode, kan du ignorere denne besked.";
            await user.SendMessageAsync(message);
            Console.WriteLine($"Verification kode sendt til Discord bruger: {discordId}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Kunne ikke sende verification kode til {discordId}: {ex.Message}");
            return false;
        }
    }

    public async Task StopAsync()
    {
        _isConnected = false;
        _isReady = false;
        await _client.LogoutAsync();
        await _client.StopAsync();
    }

    public async Task<ulong> SendSubmissionForApprovalAsync(KnowledgeSubmission submission)
    {
        UpdateGatewayActivity();

        var channel = _client.GetChannel(_modChannelId) as IMessageChannel;

        if (channel == null)
        {
            throw new InvalidOperationException($"Mod Channel med {_modChannelId} kunne ikke findes");
        }

        var message = await channel.SendMessageAsync(BuildModerationMessageContent(submission));
        return message.Id;
    }

    public async Task<ulong> PublishApprovedSubmissionAsync(KnowledgeSubmission submission)
    {
        UpdateGatewayActivity();
        var channel = _client.GetChannel(_knowledgeCenterChannelId) as IMessageChannel;

        if (channel == null)
        {
            throw new InvalidOperationException($"Knowledge Center Channel med {_knowledgeCenterChannelId} kunne ikke findes");
        }

        var message = await channel.SendMessageAsync(BuildPublishedMessageContent(submission));
        await AwardKnowledgeCenterXpAsync(submission.DiscordId);
        return message.Id;
    }

    public async Task KnowledgeCenterPostReaction(SocketReaction reaction, Cacheable<IUserMessage, ulong> cachedMessage)
    {
        UpdateGatewayActivity();

        var message = await cachedMessage.GetOrDownloadAsync();

        if (reaction.Emote.Name != "👎" && reaction.Emote.Name != "👍")
        {
            return;
        }

        if (!message.Author.IsBot)
        {
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var submission = await dbContext.KnowledgeSubmissions
            .FirstOrDefaultAsync(s => s.ModMessageId == message.Id);

        if (submission == null)
        {
            Console.WriteLine($"Kunne ikke finde submission for mod-message id {message.Id}");
            return;
        }

        if (submission.Status != KnowledgeSubmissionStatus.Pending)
        {
            return;
        }

        if (reaction.Emote.Name == "👍")
        {
            submission.Status = KnowledgeSubmissionStatus.Approved;
            submission.ReviewedByUserId = reaction.UserId.ToString();
            submission.ReviewedAt = DateTime.UtcNow;
            submission.RejectionReason = null;
            submission.PublishedMessageId = await PublishApprovedSubmissionAsync(submission);
            submission.PublishedToDiscordAt = DateTime.UtcNow;
        }
        else
        {
            submission.Status = KnowledgeSubmissionStatus.Rejected;
            submission.ReviewedByUserId = reaction.UserId.ToString();
            submission.ReviewedAt = DateTime.UtcNow;
            submission.RejectionReason = "Afvist via Discord moderation.";
        }

        submission.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();
        await message.DeleteAsync();
    }

    private string BuildModerationMessageContent(KnowledgeSubmission submission)
    {
        var authorLabel = FormatKnowledgeCenterAuthorLine(submission);

        return
            $"<@&{_modRoleId}>\r\n"
            + $"**Submission ID:** `{submission.Id}`\r\n"
            + $"**Nyt materiale af:** {authorLabel}\r\n"
            + $"**Materiale type:** {submission.Type}\r\n\r\n"
            + $"**Titel:** {submission.Title}\r\n"
            + $"**Beskrivelse:** {submission.Description}\r\n"
            + $"**Link:** {(string.IsNullOrWhiteSpace(submission.LinkToPost) ? "Intet link angivet" : submission.LinkToPost)}";
    }

    private string BuildPublishedMessageContent(KnowledgeSubmission submission)
    {
        var author = FormatKnowledgeCenterAuthorLine(submission);
        var (typeEmoji, typeLabel, linkIntro) = MapKnowledgeSubmissionTypeParts(submission.Type);
        var linkValue = string.IsNullOrWhiteSpace(submission.LinkToPost)
            ? "Intet link angivet"
            : submission.LinkToPost.Trim();

        // Discord: # / ## = større skrift, ** = fed (jf. Discord markdown)
        return "# 🪐 **Nyt materiale udgivet af:** "
            + $"{author}, **Tjek det ud!**\r\n\r\n"
            + $"## **{typeEmoji} Materiale type:** {typeLabel}\r\n\r\n"
            + $"## **📌 Titel:** {submission.Title}\r\n\r\n"
            + "## **✉️ Beskrivelse:**\r\n"
            + $"{submission.Description}\r\n\r\n"
            + $"## **🔗 {linkIntro}:** {linkValue}";
    }

    /// <summary>
    /// Discord mention når vi har et gyldigt snowflake; ellers SSO-/visningsnavn.
    /// </summary>
    private static string FormatKnowledgeCenterAuthorLine(KnowledgeSubmission submission)
    {
        var discordId = submission.DiscordId?.Trim() ?? string.Empty;
        if (!string.IsNullOrEmpty(discordId) && ulong.TryParse(discordId, out _))
        {
            return $"<@{discordId}>";
        }

        return string.IsNullOrWhiteSpace(submission.AuthorName) ? "Ukendt bruger" : submission.AuthorName;
    }

    private static (string Emoji, string TypeLabel, string LinkIntro) MapKnowledgeSubmissionTypeParts(string? typeRaw)
    {
        var type = (typeRaw ?? string.Empty).Trim().ToLowerInvariant();
        return type switch
        {
            "blog-post" => ("📝", "Blogindlæg", "Link til blogindlæg"),
            "video" => ("🎥", "Video", "Link til video"),
            "artikel" => ("📰", "Artikel", "Link til artikel"),
            "andet" => ("📎", "Andet", "Link til materiale"),
            _ => ("📝", typeRaw?.Trim() ?? "Ukendt", $"Link til {typeRaw?.Trim() ?? "materiale"}"),
        };
    }

    private async Task AwardKnowledgeCenterXpAsync(string discordId)
    {
        if (string.IsNullOrWhiteSpace(discordId))
        {
            return;
        }

        var match = Regex.Match(discordId, @"\d+");
        if (!match.Success)
        {
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var xpService = scope.ServiceProvider.GetRequiredService<XPService>();
        await xpService.AddXPAsync(match.Value, XpActivityType.KnowledgeCenterApproved);
    }

    private Task HandleConnectedAsync()
    {
        _isConnected = true;
        UpdateGatewayActivity();
        return Task.CompletedTask;
    }

    private Task HandleDisconnectedAsync(Exception exception)
    {
        _isConnected = false;
        _isReady = false;
        _lastDisconnectUtc = DateTime.UtcNow;
        _lastDisconnectReason = exception?.Message ?? "Unknown disconnect reason";
        return Task.CompletedTask;
    }

    private Task HandleLatencyUpdatedAsync(int oldLatency, int newLatency)
    {
        UpdateGatewayActivity();
        return Task.CompletedTask;
    }

    private void UpdateGatewayActivity()
    {
        _lastGatewayActivityUtc = DateTime.UtcNow;
    }
}

public class DiscordHealthSnapshot
{
    public bool IsConnected { get; set; }
    public bool IsReady { get; set; }
    public DateTime? LastGatewayActivityUtc { get; set; }
    public DateTime? LastReadyUtc { get; set; }
    public DateTime? LastDisconnectUtc { get; set; }
    public string? LastDisconnectReason { get; set; }
    public string ConnectionState { get; set; } = "Unknown";
    public string LoginState { get; set; } = "Unknown";
}

public class DiscordHostedService : IHostedService
{
    private readonly DiscordBotService _discordBotService;

    public DiscordHostedService(DiscordBotService discordBotService)
    {
        _discordBotService = discordBotService;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _discordBotService.StartAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _discordBotService.StopAsync();
    }
}
