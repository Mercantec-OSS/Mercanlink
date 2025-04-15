using Backend.DiscordBot.Commands;
using Backend.DiscordServices.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

public class DiscordBotService
{
    private readonly DiscordSocketClient _client;
    private readonly string _token;
    private readonly string _prefix = "!"; // Prefix for kommandoer
    private readonly IServiceProvider _serviceProvider;
    private readonly XPService _xpService;
    private readonly Dictionary<ulong, DateTime> _voiceUsers = new Dictionary<ulong, DateTime>();

    public DiscordBotService(IConfiguration config, IServiceProvider serviceProvider)
    {
        _token = config["Discord:Token"];
        _client = new DiscordSocketClient(
            new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent | GatewayIntents.GuildMembers
            }
        );

        _serviceProvider = serviceProvider;

        // Sæt service provider i Commands klassen
        Commands.SetServiceProvider(serviceProvider);
    }

    public async Task StartAsync()
    {
        _client.Log += LogAsync;
        _client.Ready += () =>
        {
            Console.WriteLine($"Bot er forbundet til {_client.Guilds.Count} servere!");
            foreach (var guild in _client.Guilds)
            {
                Console.WriteLine($" - {guild.Name} (ID: {guild.Id})");
            }
            return Task.CompletedTask;
        };

        // Tilføj message handler
        _client.MessageReceived += HandleCommandAsync;

        // Tilføj XP event handlers
        _client.MessageReceived += HandleMessageXpAsync;
        _client.ReactionAdded += HandleReactionXpAsync;
        _client.UserVoiceStateUpdated += HandleVoiceXpAsync;

        // Registrer bruger 
        _client.UserJoined += HandleRegisterAsync;

        await _client.LoginAsync(TokenType.Bot, _token);
        await _client.StartAsync();
    }

    private async Task HandleCommandAsync(SocketMessage messageParam)
    {
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

    // XP for beskeder
    private async Task HandleMessageXpAsync(SocketMessage message)
    {
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
            await xpService.AddXPAsync(message.Author.Id.ToString(), XPActivityType.Message);
        }
    }

    // XP for reaktioner
    private async Task HandleReactionXpAsync(
        Cacheable<IUserMessage, ulong> cachedMessage,
        Cacheable<IMessageChannel, ulong> cachedChannel,
        SocketReaction reaction
    )
    {
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
                await xpService.AddXPAsync(reaction.UserId.ToString(), XPActivityType.Reaction);
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
                                XPActivityType.VoiceMinute
                            );
                        }
                    }
                }

                _voiceUsers.Remove(user.Id);
            }
        }
    }

    private async Task HandleRegisterAsync(SocketGuildUser guildUser)
    {
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
                .WithTitle(isNewUser ? "Velkommen til Mercantec Space!" : "Du er allerede registreret!")
                .WithDescription(
                    isNewUser
                        ? "Din Discord-konto er nu registreret i vores system. Du kan nu optjene XP og stige i level!"
                        : "Din Discord-konto er allerede registreret i vores system."
                )
                .WithColor(isNewUser ? Color.Green : Color.Blue)
                .WithThumbnailUrl(guildUser.GetAvatarUrl() ?? guildUser.GetDefaultAvatarUrl())
                .WithCurrentTimestamp();

            if (isNewUser)
            {
                await xpService.AddXPAsync(guildUser.Id.ToString(), XPActivityType.DailyLogin);
                embed.AddField("Næste skridt", "Senere vil du kunne forbinde din konto med vores hjemmeside for at få adgang til flere funktioner.");
                embed.AddField("XP System", "Du optjener XP ved at være aktiv på serveren. Brug !rank for at se dit level og XP.");
            }

            await guildUser.SendMessageAsync(embed: embed.Build());
        }
        catch (Exception ex)
        {
            await guildUser.SendMessageAsync($"Der opstod en fejl under registrering: {ex.Message}");
        }
    }

    private Task LogAsync(LogMessage log)
    {
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

    public async Task StopAsync()
    {
        await _client.LogoutAsync();
        await _client.StopAsync();
    }
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
