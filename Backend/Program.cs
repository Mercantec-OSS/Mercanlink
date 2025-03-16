namespace Backend;

using Backend.Data;
using Backend.DiscordServices.Services;
using Microsoft.EntityFrameworkCore;
using Backend.Models;
using Backend.Jobs;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();

        // Add services to the container.
        builder.Services.AddControllers();

        // Tilføj PostgreSQL DbContext
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
        );

        // Tilføj XP konfiguration
        builder.Services.Configure<XPConfig>(options =>
        {
            // Direkte indlæsning af konfiguration hvis filen ikke findes
            if (builder.Configuration.GetSection("XPConfig").GetChildren().Count() == 0)
            {
                options.ActivityRewards = new Dictionary<string, int>
                {
                    { "Message", 10 },
                    { "Reaction", 5 },
                    { "VoiceMinute", 15 },
                    { "CommandUsed", 2 },
                    { "DailyLogin", 25 }
                };
                options.ActivityCooldowns = new Dictionary<string, int>
                {
                    { "Message", 60 },
                    { "Reaction", 30 },
                    { "VoiceMinute", 0 },
                    { "CommandUsed", 10 },
                    { "DailyLogin", 86400 }
                };
                options.DailyLimits = new Dictionary<string, int>
                {
                    { "Message", 100 },
                    { "Reaction", 50 },
                    { "VoiceMinute", 120 },
                    { "CommandUsed", 50 },
                    { "DailyLogin", 1 }
                };
                options.BaseXP = 100;
                options.LevelMultiplier = 1.5;
            }
            else
            {
                builder.Configuration.GetSection("XPConfig").Bind(options);
            }

            var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
            logger.LogInformation("XP Config indlæst: {ActivityRewards} aktiviteter konfigureret",
                options.ActivityRewards?.Count ?? 0);
        });

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        builder.Services.AddHostedService<DiscordHostedService>();
        builder.Services.AddSingleton<DiscordBotService>(provider =>
            new DiscordBotService(builder.Configuration, provider));
        builder.Services.AddScoped<UserService>();
        builder.Services.AddScoped<XPService>();
        builder.Services.AddSingleton<LevelSystem>();

        // Tilføj cleanup job
        builder.Services.AddHostedService<CleanupJob>();

        // Tilføj logging konfiguration
        builder.Logging.AddConsole();
        builder.Logging.SetMinimumLevel(LogLevel.Information);

        var app = builder.Build();

        // Log at Discord botten starter
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Discord bot starter op...");

        app.MapDefaultEndpoints();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
