namespace Backend.DiscordBot.Commands;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Backend.DiscordServices.Services;
using Microsoft.Extensions.Logging;
using Backend.Data;
using Backend.Models;

public partial class Commands
{
    private static IServiceProvider _serviceProvider;

    // Sæt service provider
    public static void SetServiceProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    // Dictionary til at holde alle kommandoer
    private static readonly Dictionary<
        string,
        Func<SocketMessage, DiscordSocketClient, Task>
    > _commands =
        new()
        {
            { "ping", PingCommand },
            { "hello", HejCommand },
            { "help", HjælpCommand },
            { "info", InfoCommand },
            { "rank", RankCommand },
            { "register", RegisterCommand },
            { "givexp", GiveXPCommand },
            { "testxp", TestXPCommand },
            { "daily", DailyCommand }
        };

    // Metode til at hente alle kommandoer 
    public static Dictionary<string, Func<SocketMessage, DiscordSocketClient, Task>> GetCommands()
    {
        return _commands;
    }

    // Metode til at tjekke om en kommando eksisterer
    public static bool CommandExists(string command)
    {
        return _commands.ContainsKey(command.ToLower());
    }

    // Metode til at udføre en kommando
    public static async Task ExecuteCommand(
        string command,
        SocketMessage message,
        DiscordSocketClient client
    )
    {
        if (CommandExists(command))
        {
            await _commands[command.ToLower()](message, client);
        }
    }

    // Ping kommando - !ping
    private static async Task PingCommand(SocketMessage message, DiscordSocketClient client)
    {
        await message.Channel.SendMessageAsync("Pong! 🏓");
    }

    // Hej kommando
    private static async Task HejCommand(SocketMessage message, DiscordSocketClient client)
    {
        await message.Channel.SendMessageAsync($"Hej {message.Author.Mention}! 👋");
    }

    // Hjælp kommando - !help
    private static async Task HjælpCommand(SocketMessage message, DiscordSocketClient client)
    {
        string prefix = "!";

        var embedBuilder = new EmbedBuilder()
            .WithTitle("Kommando Hjælp")
            .WithDescription("Her er en liste over alle tilgængelige kommandoer:")
            .WithColor(Color.Blue)
            .AddField($"{prefix}ping", "Tjek om botten er online")
            .AddField($"{prefix}hello", "Få en hilsen fra botten")
            .AddField($"{prefix}help", "Vis denne hjælpebesked")
            .AddField($"{prefix}info", "Vis information om botten")
            .AddField($"{prefix}rank", "Vis din rang og XP")
            .AddField($"{prefix}register", "Registrer din Discord-konto")
            .AddField($"{prefix}givexp", "Giv XP til en bruger")
            .AddField($"{prefix}testxp", "Test XP-systemet")
            .AddField($"{prefix}daily", "Få daglig XP-bonus")
            .WithFooter(footer => footer.Text = "Brug præfiks ! før hver kommando");

        await message.Channel.SendMessageAsync(embed: embedBuilder.Build());
    }

    // Info kommando - !info
    private static async Task InfoCommand(SocketMessage message, DiscordSocketClient client)
    {
        var embed = new EmbedBuilder()
            .WithTitle("Bot Information")
            .WithDescription("En simpel Discord bot lavet med Discord.NET")
            .WithColor(Color.Blue)
            .WithCurrentTimestamp()
            .WithFooter(footer => footer.Text = "Lavet af Mercantec Space Teamet")
            .AddField("Servere", client.Guilds.Count, true)
            .AddField("Ping", $"{client.Latency} ms", true)
            .Build();

        await message.Channel.SendMessageAsync(embed: embed);
    }

    // Rank kommando - !rank
    private static async Task RankCommand(SocketMessage message, DiscordSocketClient client)
    {
        if (_serviceProvider == null)
        {
            await message.Channel.SendMessageAsync("Fejl: Service provider er ikke konfigureret.");
            return;
        }

        using (var scope = _serviceProvider.CreateScope())
        {
            var xpService = scope.ServiceProvider.GetRequiredService<XPService>();
            var progress = await xpService.GetUserProgressAsync(message.Author.Id.ToString());
            var stats = await xpService.GetUserActivityStatsAsync(message.Author.Id.ToString());

            var embed = new EmbedBuilder()
                .WithTitle($"{message.Author.Username}'s Rank")
                .WithDescription($"Level: {progress.Level}\nXP: {progress.XP}/{progress.RequiredXP}")
                .WithColor(Color.Gold)
                .WithThumbnailUrl(message.Author.GetAvatarUrl() ?? message.Author.GetDefaultAvatarUrl())
                .WithCurrentTimestamp();

            // Tilføj daglige aktiviteter
            var activityField = "";
            foreach (var stat in stats)
            {
                activityField += $"{stat.Key}: {stat.Value} i dag\n";
            }

            if (!string.IsNullOrEmpty(activityField))
            {
                embed.AddField("Dagens aktiviteter", activityField);
            }

            await message.Channel.SendMessageAsync(embed: embed.Build());
        }
    }

    // Register kommando - !register
    private static async Task RegisterCommand(SocketMessage message, DiscordSocketClient client)
    {
        if (_serviceProvider == null)
        {
            await message.Channel.SendMessageAsync("Fejl: Service provider er ikke konfigureret.");
            return;
        }

        // Tjek om beskeden er fra en guild
        if (message.Channel is not SocketGuildChannel guildChannel)
        {
            await message.Channel.SendMessageAsync("Denne kommando kan kun bruges på en server.");
            return;
        }

        // Få fat i guild user
        var guildUser = (message.Author as SocketGuildUser);
        if (guildUser == null)
        {
            await message.Channel.SendMessageAsync("Kunne ikke finde dig som server-medlem.");
            return;
        }

        using (var scope = _serviceProvider.CreateScope())
        {
            var userService = scope.ServiceProvider.GetRequiredService<UserService>();
            var xpService = scope.ServiceProvider.GetRequiredService<XPService>();

            try
            {
                // Opret eller hent bruger
                var user = await userService.CreateDiscordUserAsync(guildUser);
                bool isNewUser = user.CreatedAt > DateTime.UtcNow.AddMinutes(-1);

                // Giv XP for registrering hvis det er en ny bruger
                if (isNewUser)
                {
                    await xpService.AddXPAsync(guildUser.Id.ToString(), XPActivityType.DailyLogin);
                }

                // Byg embed besked
                var embed = new EmbedBuilder()
                    .WithTitle(isNewUser ? "Velkommen til Mercantec Space!" : "Du er allerede registreret!")
                    .WithDescription(isNewUser
                        ? "Din Discord-konto er nu registreret i vores system. Du kan nu optjene XP og stige i level!"
                        : "Din Discord-konto er allerede registreret i vores system.")
                    .WithColor(isNewUser ? Color.Green : Color.Blue)
                    .WithThumbnailUrl(guildUser.GetAvatarUrl() ?? guildUser.GetDefaultAvatarUrl())
                    .WithCurrentTimestamp();

                if (isNewUser)
                {
                    embed.AddField("Næste skridt", "Senere vil du kunne forbinde din konto med vores hjemmeside for at få adgang til flere funktioner.");
                    embed.AddField("XP System", "Du optjener XP ved at være aktiv på serveren. Brug !rank for at se dit level og XP.");
                }

                await message.Channel.SendMessageAsync(embed: embed.Build());
            }
            catch (Exception ex)
            {
                await message.Channel.SendMessageAsync($"Der opstod en fejl: {ex.Message}");
            }
        }
    }

    // GiveXP kommando - !givexp
    private static async Task GiveXPCommand(SocketMessage message, DiscordSocketClient client)
    {
        if (_serviceProvider == null)
        {
            await message.Channel.SendMessageAsync("Fejl: Service provider er ikke konfigureret.");
            return;
        }

        using (var scope = _serviceProvider.CreateScope())
        {
            var xpService = scope.ServiceProvider.GetRequiredService<XPService>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Commands>>();

            try
            {
                // Giv XP for en besked
                bool success = await xpService.AddXPAsync(message.Author.Id.ToString(), XPActivityType.Message);

                if (success)
                {
                    var progress = await xpService.GetUserProgressAsync(message.Author.Id.ToString());
                    await message.Channel.SendMessageAsync($"Du fik XP! Du er nu level {progress.Level} med {progress.XP}/{progress.RequiredXP} XP.");
                }
                else
                {
                    await message.Channel.SendMessageAsync("Kunne ikke give XP. Tjek logs for detaljer.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Fejl ved givexp kommando");
                await message.Channel.SendMessageAsync($"Der opstod en fejl: {ex.Message}");
            }
        }
    }

    // TestXP kommando - !testxp
    private static async Task TestXPCommand(SocketMessage message, DiscordSocketClient client)
    {
        if (_serviceProvider == null)
        {
            await message.Channel.SendMessageAsync("Fejl: Service provider er ikke konfigureret.");
            return;
        }

        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Commands>>();
            var userService = scope.ServiceProvider.GetRequiredService<UserService>();

            try
            {
                // Find brugeren
                var user = await userService.GetUserByDiscordIdAsync(message.Author.Id.ToString());
                if (user == null)
                {
                    await message.Channel.SendMessageAsync("Du er ikke registreret. Brug !register først.");
                    return;
                }

                // Tilføj XP direkte til brugeren
                user.Experience += 10;
                dbContext.Users.Update(user);

                // Opret en aktivitetspost direkte
                var dailyActivity = new UserDailyActivity
                {
                    UserId = user.Id,
                    ActivityType = "TestXP",
                    Date = DateTime.UtcNow.Date,
                    Count = 1,
                    TotalXPAwarded = 10,
                    LastActivity = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                dbContext.Set<UserDailyActivity>().Add(dailyActivity);

                // Gem ændringerne
                await dbContext.SaveChangesAsync();

                // Bekræft at det virkede
                await message.Channel.SendMessageAsync($"Test XP tilføjet! Du har nu {user.Experience} XP. Tjek !rank for at se om aktiviteten blev registreret.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Fejl ved testxp kommando");
                await message.Channel.SendMessageAsync($"Der opstod en fejl: {ex.Message}");
            }
        }
    }
    // Daily kommando - !daily (opdateret)
    private static async Task DailyCommand(SocketMessage message, DiscordSocketClient client)
    {
        if (_serviceProvider == null)
        {
            await message.Channel.SendMessageAsync("Fejl: Service provider er ikke konfigureret.");
            return;
        }

        using (var scope = _serviceProvider.CreateScope())
        {
            var xpService = scope.ServiceProvider.GetRequiredService<XPService>();

            try
            {
                bool success = await xpService.AddXPAsync(message.Author.Id.ToString(), XPActivityType.DailyLogin);

                if (success)
                {
                    var progress = await xpService.GetUserProgressAsync(message.Author.Id.ToString());
                    await message.Channel.SendMessageAsync($"Du har fået din daglige XP bonus! Du er nu level {progress.Level} med {progress.XP}/{progress.RequiredXP} XP.");
                }
                else
                {
                    await message.Channel.SendMessageAsync("Du har allerede fået din daglige XP bonus i dag. Kom tilbage i morgen!");
                }
            }
            catch (Exception ex)
            {
                await message.Channel.SendMessageAsync($"Der opstod en fejl: {ex.Message}");
            }
        }
    }
}
