namespace Backend.DiscordBot.Commands;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

public partial class Commands
{
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
            { "info", InfoCommand }
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

    // Ping kommando
    private static async Task PingCommand(SocketMessage message, DiscordSocketClient client)
    {
        await message.Channel.SendMessageAsync("Pong! 🏓");
    }

    // Hej kommando
    private static async Task HejCommand(SocketMessage message, DiscordSocketClient client)
    {
        await message.Channel.SendMessageAsync($"Hej {message.Author.Mention}! 👋");
    }

    // Hjælp kommando
    private static async Task HjælpCommand(SocketMessage message, DiscordSocketClient client)
    {
        string prefix = "!";

        var embedBuilder = new EmbedBuilder()
            .WithTitle("Command Help")
            .WithDescription("Here is a list of all available commands:")
            .WithColor(Color.Blue)
            .AddField($"{prefix}ping", "Check if the bot is online")
            .AddField($"{prefix}hello", "Get a greeting from the bot")
            .AddField($"{prefix}help", "Show this help message")
            .AddField($"{prefix}info", "Show information about the bot")
            .WithFooter(footer => footer.Text = "Use prefix ! before each command");

        await message.Channel.SendMessageAsync(embed: embedBuilder.Build());
    }

    // Info kommando
    private static async Task InfoCommand(SocketMessage message, DiscordSocketClient client)
    {
        var embed = new EmbedBuilder()
            .WithTitle("Bot Information")
            .WithDescription("En simpel Discord bot lavet med Discord.NET")
            .WithColor(Color.Blue)
            .WithCurrentTimestamp()
            .WithFooter(footer => footer.Text = "Lavet af Mercantec Space")
            .AddField("Servere", client.Guilds.Count, true)
            .AddField("Ping", $"{client.Latency} ms", true)
            .Build();

        await message.Channel.SendMessageAsync(embed: embed);
    }
}
