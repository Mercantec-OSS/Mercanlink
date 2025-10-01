using Backend.DiscordBot.Commands;
using Backend.DiscordServices.Services;
using Backend.Models.DTOs;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Channels;

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
    private readonly XPService _xpService;
    private readonly Dictionary<ulong, DateTime> _voiceUsers = new Dictionary<ulong, DateTime>();

    public DiscordBotService(IConfiguration config, IServiceProvider serviceProvider)
    {
        _token = Environment.GetEnvironmentVariable("DISCORD_TOKEN") ?? config["Discord:Token"];
        _modChannelId = Convert.ToUInt64( Environment.GetEnvironmentVariable("DISCORD_MODCHANNELID") ??config["Discord:ModChannelId"]);
        _knowledgeCenterChannelId = Convert.ToUInt64( Environment.GetEnvironmentVariable("DISCORD_KNOWLEDGECENTERCHANNELID") ?? config["Discord:KnowledgeCenterChannelId"]);
        _modRoleId = Convert.ToUInt64( Environment.GetEnvironmentVariable("DISCORD_MODROLEID") ?? config["Discord:ModRoleId"]);
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
        _client.Log += LogAsync;
        _client.Ready += async () =>
        {
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

    public async Task HandleRegisterAsync(SocketGuildUser guildUser)
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
                await xpService.AddXPAsync(guildUser.Id.ToString(), XPActivityType.DailyLogin);
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
        await _client.LogoutAsync();
        await _client.StopAsync();
    }

    public async Task KnowledgeCenterPostApproval(PostDTO post)
    {

        var channel = _client.GetChannel(_modChannelId) as IMessageChannel;

        if (channel == null)
        {
            Console.WriteLine($"Channel med {_modChannelId.ToString()} kunne ikke findes");
        }
        else
        {
            await channel.SendMessageAsync(
                $"<@&{_modRoleId}>\r\n**" +
                $"Nyt materiale er blevet udgivet af:** {post.Author} (<@{post.DiscordId}>)\r\n**" +
                $"Materiale type:** {post.Type}\r\n\r\n**" +
                $"Titel:** {post.Title}\r\n**" +
                $"Beskrivelse:** {post.Description}\r\n" +
                $"\r\n**" +
                $"Link:** {post.LinkToPost}");
        }
    }

    public async Task KnowledgeCenterPostReaction(SocketReaction reaction, Cacheable<IUserMessage, ulong> cachedMessage)
    {
        var message = await cachedMessage.GetOrDownloadAsync();
        string newMessageContent = message.Content.Replace($"<@&{_modRoleId}>", "@everyone");

        var channel = _client.GetChannel(_knowledgeCenterChannelId) as IMessageChannel;

        if (reaction.Emote.Name == "👎" || reaction.Emote.Name == "👍")
        {
            if (channel == null || !message.Author.IsBot)
            {
                Console.WriteLine($"Channel med {_knowledgeCenterChannelId.ToString()} kunne ikke findes");
            }
            else
            {
                if (reaction.Emote.Name != "👎")
                {
                    await channel.SendMessageAsync(newMessageContent);
                }
                await message.DeleteAsync();
            }
        }
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
