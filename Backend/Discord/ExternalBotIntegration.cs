using Newtonsoft.Json.Linq;
using System.Text;
using System.Text.Json;

namespace Backend.Discord
{

    public class ExternalBotIntegration
    {
        private readonly string _apiKey;
        private readonly ulong _serverId;
        public ExternalBotIntegration(IConfiguration config)
        {
            _apiKey = Environment.GetEnvironmentVariable("DISCORD_LURKRAPIKEY") ?? config["Discord:LurkrAPIKey"];
            _serverId = Convert.ToUInt64(Environment.GetEnvironmentVariable("DISCORD_SERVERID") ?? config["Discord:ServerId"]);
        }

        public async Task Addexp(int xpIncrease, ulong discordId)
        {
            using var client = new HttpClient();

            client.DefaultRequestHeaders.Add("X-Api-Key", $"{_apiKey}");
            var body = new
            {
                xp = new
                {
                    increment = xpIncrease,
                }
            };

            var apiUrl = $"https://api.lurkr.gg/v2/levels/{_serverId}/users/{discordId}";

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PatchAsync(apiUrl, content);
            Console.WriteLine($"Status Code: {response.StatusCode}");
            Console.WriteLine("Response:");
            Console.WriteLine(response.Content.ToString());
        }
    }
}
