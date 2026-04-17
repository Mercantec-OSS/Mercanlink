using Backend.Discord.Enums;

namespace Backend;

using Backend.Data;
using Backend.DiscordServices.Services;
using Microsoft.EntityFrameworkCore;
using Backend.Models;
using Backend.Jobs;
using Backend.Config;
using Backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Backend.DBAccess;
using Backend.Discord;
using System.Security.Claims;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.        
        builder.Services.AddControllers();        
        // Tilføj CORS        
        builder.Services.AddCors(options =>        
        {            
            options.AddPolicy("AllowFrontend", policy =>            
            {                
                policy.WithOrigins("http://localhost:5173", "http://localhost:3000", "http://localhost:4200", "https://hub.mercantec.tech")                      
                .AllowAnyHeader()                      
                .AllowAnyMethod()                      
                .AllowCredentials();            
            });        
        });

        // Tilføj PostgreSQL DbContext
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(Environment.GetEnvironmentVariable("DEFAULT_CONNECTION") ?? builder.Configuration.GetConnectionString("DefaultConnection"))
        );

        // Tilføj Mercantec Auth konfiguration
        var mercantecAuthConfig = new MercantecAuthConfig
        {
            Issuer =
                Environment.GetEnvironmentVariable("MERCANTEC_AUTH_ISSUER")
                ?? builder.Configuration["MercantecAuth:Issuer"]
                ?? "https://auth.mercantec.tech",

            Audience =
                Environment.GetEnvironmentVariable("MERCANTEC_AUTH_AUDIENCE")
                ?? builder.Configuration["MercantecAuth:Audience"]
                ?? "mercantec-apps",

            JwksUri =
                Environment.GetEnvironmentVariable("MERCANTEC_AUTH_JWKS_URI")
                ?? builder.Configuration["MercantecAuth:JwksUri"]
                ?? "https://auth.mercantec.tech/.well-known/jwks.json"
        };

        builder.Services.Configure<MercantecAuthConfig>(options =>
        {
            options.Issuer = mercantecAuthConfig.Issuer;
            options.Audience = mercantecAuthConfig.Audience;
            options.JwksUri = mercantecAuthConfig.JwksUri;
        });

        var jwksKeyProvider = new MercantecJwksKeyProvider(mercantecAuthConfig.JwksUri);

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.MapInboundClaims = false;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKeyResolver = (_, _, _, _) => jwksKeyProvider.GetSigningKeys(),
                ValidateIssuer = true,
                ValidIssuer = mercantecAuthConfig.Issuer,
                ValidateAudience = true,
                ValidAudience = mercantecAuthConfig.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                NameClaimType = ClaimTypes.NameIdentifier,
                RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
            };

            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = context =>
                {
                    if (context.Principal?.Identity is ClaimsIdentity identity)
                    {
                        var roleClaimTypes = new[]
                        {
                            "role",
                            ClaimTypes.Role,
                            "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
                        };

                        var roles = identity.Claims
                            .Where(c => roleClaimTypes.Contains(c.Type))
                            .Select(c => c.Value)
                            .Distinct()
                            .ToList();

                        foreach (var role in roles)
                        {
                            if (!identity.HasClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", role))
                            {
                                identity.AddClaim(new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", role));
                            }

                            if (!identity.HasClaim(ClaimTypes.Role, role))
                            {
                                identity.AddClaim(new Claim(ClaimTypes.Role, role));
                            }
                        }
                    }

                    return Task.CompletedTask;
                }
            };
        });

        builder.Services.AddAuthorization();

        // Tilføj XP konfiguration
        builder.Services.Configure<XpConfig>(options =>
        {
            // Direkte indlæsning af konfiguration hvis filen ikke findes
            if (builder.Configuration.GetSection("XPConfig").GetChildren().Count() == 0)
            {
                options.BaseXP = 100;
                options.LevelMultiplier = 1.5;
            }
            else
            {
                builder.Configuration.GetSection("XPConfig").Bind(options);
            }

            // XP config logges senere når app'en er bygget og logger er tilgængelig via DI.
        });

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();
        
        // Tilføj Swagger med XML kommentarer
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo 
            { 
                Title = "Mercantec-Space API", 
                Version = "v1",
                Description = "En platform til socialt og fagligt fællesskab for nuværende og tidligere elever på Mercantec"
            });
            
            // Inkluder XML kommentarer
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }
            
            // JWT Authentication i Swagger
            c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Eksempel: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });
            
            c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement()
            {
                {
                    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Reference = new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    },
                    new List<string>()
                }
            });
        });

        // Tilføj Discord services        
        builder.Services.AddHostedService<DiscordHostedService>();        
        builder.Services.AddSingleton<DiscordBotService>(provider =>            
        new DiscordBotService(builder.Configuration, provider));
        builder.Services.AddScoped<ExternalBotIntegration>();
        builder.Services.AddScoped<UserService>();        
        builder.Services.AddScoped<XPService>();        
        builder.Services.AddScoped<LevelSystem>();
        // Tilføj authentication services        
        builder.Services.AddScoped<AuthService>();        
        builder.Services.AddScoped<DiscordVerificationService>();
        // Tilføj DBAccess        
        builder.Services.AddScoped<AuthDBAccess>();
        builder.Services.AddScoped<DiscordVerificationDBAccess>();
        builder.Services.AddScoped<DiscordBotDBAccess>();
        builder.Services.AddScoped<UserDBAccess>();

        // Tilføj cleanup job
        builder.Services.AddHostedService<CleanupJob>();

        // Tilføj logging konfiguration
        builder.Logging.AddConsole();
        builder.Logging.SetMinimumLevel(LogLevel.Information);

        // Tilføj HttpClient til DI container
        builder.Services.AddHttpClient();

        var discordHealthStaleSeconds = int.TryParse(
            Environment.GetEnvironmentVariable("DISCORD_HEALTH_STALE_SECONDS"),
            out var configuredStaleSeconds
        )
            ? configuredStaleSeconds
            : 180;

        var app = builder.Build();

        // Log at Discord botten starter
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Discord bot starter op...");

        // Kør migrationer automatisk ved deploy/startup.
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            const int maxMigrationAttempts = 10;
            const int delayBetweenAttemptsMs = 5000;

            for (var attempt = 1; attempt <= maxMigrationAttempts; attempt++)
            {
                try
                {
                    dbContext.Database.Migrate();
                    logger.LogInformation("Database migrationer gennemført.");
                    break;
                }
                catch (Exception ex) when (attempt < maxMigrationAttempts)
                {
                    logger.LogWarning(
                        ex,
                        "Migration forsøg {Attempt}/{MaxAttempts} fejlede. Prøver igen om {DelaySeconds} sekunder.",
                        attempt,
                        maxMigrationAttempts,
                        delayBetweenAttemptsMs / 1000
                    );
                    Thread.Sleep(delayBetweenAttemptsMs);
                }
            }
        }

        // Configure the HTTP request pipeline.
        app.MapOpenApi();
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mercantec-Space API v1");
            c.RoutePrefix = "swagger"; // Swagger UI vil være tilgængelig på /swagger
        });
        

        app.UseHttpsRedirection();

        // Tilføj CORS - skal være før Authentication og Authorization
        app.UseCors("AllowFrontend");

        // Vigtig rækkefølge: Authentication før Authorization
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapGet("/health", async (ApplicationDbContext dbContext, DiscordBotService discordBotService, CancellationToken cancellationToken) =>
        {
            var dbHealthy = await dbContext.Database.CanConnectAsync(cancellationToken);
            var discord = discordBotService.GetHealthSnapshot();
            var nowUtc = DateTime.UtcNow;
            var staleAfter = TimeSpan.FromSeconds(discordHealthStaleSeconds);
            var freshnessProbeUtc = discord.LastGatewayActivityUtc ?? discord.LastReadyUtc;
            var gatewayIsFresh = freshnessProbeUtc.HasValue
                && nowUtc - freshnessProbeUtc.Value <= staleAfter;
            var discordHealthy = discord.IsConnected && discord.IsReady && gatewayIsFresh;
            var overallHealthy = dbHealthy && discordHealthy;

            var payload = new
            {
                status = overallHealthy ? "healthy" : "unhealthy",
                checkedAtUtc = nowUtc,
                checks = new
                {
                    api = new { ok = true },
                    database = new { ok = dbHealthy },
                    discord = new
                    {
                        ok = discordHealthy,
                        isConnected = discord.IsConnected,
                        isReady = discord.IsReady,
                        gatewayIsFresh,
                        lastGatewayActivityUtc = discord.LastGatewayActivityUtc,
                        lastReadyUtc = discord.LastReadyUtc,
                        lastDisconnectUtc = discord.LastDisconnectUtc,
                        lastDisconnectReason = discord.LastDisconnectReason,
                        connectionState = discord.ConnectionState,
                        loginState = discord.LoginState
                    }
                }
            };

            return overallHealthy
                ? Results.Ok(payload)
                : Results.Json(payload, statusCode: StatusCodes.Status503ServiceUnavailable);
        }).AllowAnonymous();

        app.MapControllers();

        app.Run();
    }
}
