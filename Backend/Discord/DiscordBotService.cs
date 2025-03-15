using Backend.DiscordBot.Commands;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

public class DiscordBotService
{
    private readonly DiscordSocketClient _client;
    private readonly string _token;
    private readonly string _prefix = "!"; // Prefix for kommandoer

    public DiscordBotService(IConfiguration config)
    {
        _token = config["Discord:Token"];
        _client = new DiscordSocketClient(
            new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
            }
        );
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
                $"Ukendt kommando. Skriv `{_prefix}hjælp` for at se tilgængelige kommandoer."
            );
        }
    }

    private Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log);
        return Task.CompletedTask;
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
