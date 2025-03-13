using Discord;
using Discord.WebSocket;

public class DiscordBotService
{
    private readonly DiscordSocketClient _client;
    private readonly string _token;

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

        await _client.LoginAsync(TokenType.Bot, _token);
        await _client.StartAsync();
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
