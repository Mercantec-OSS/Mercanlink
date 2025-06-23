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
using System.Text;
using Backend.DBAccess;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();

        // Add services to the container.        
        builder.Services.AddControllers();        
        // Tilføj CORS        
        builder.Services.AddCors(options =>        
        {            
            options.AddPolicy("AllowFrontend", policy =>            
            {                
                policy.WithOrigins("http://localhost:5173", "http://localhost:3000", "http://localhost:4200")                      
                .AllowAnyHeader()                      
                .AllowAnyMethod()                      
                .AllowCredentials();            
            });        
        });

        // Tilføj PostgreSQL DbContext
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
        );

        // Tilføj JWT konfiguration
        builder.Services.Configure<JwtConfig>(options =>
        {
            if (builder.Configuration.GetSection("JwtConfig").GetChildren().Count() == 0)
            {
                // Standard konfiguration hvis ingen findes
                options.SecretKey = builder.Configuration["JWT_SECRET"] ?? "din-super-hemmelige-noegle-der-skal-vaere-mindst-32-karakterer-lang";
                options.Issuer = "MercantecSpace";
                options.Audience = "MercantecSpaceUsers";
                options.ExpiryMinutes = 60;
                options.RefreshTokenExpiryDays = 7;
            }
            else
            {
                builder.Configuration.GetSection("JwtConfig").Bind(options);
            }
        });

        // Tilføj JWT Authentication
        var jwtConfig = new JwtConfig();
        builder.Configuration.GetSection("JwtConfig").Bind(jwtConfig);
        if (string.IsNullOrEmpty(jwtConfig.SecretKey))
        {
            jwtConfig.SecretKey = builder.Configuration["JWT_SECRET"] ?? "din-super-hemmelige-noegle-der-skal-vaere-mindst-32-karakterer-lang";
            jwtConfig.Issuer = "MercantecSpace";
            jwtConfig.Audience = "MercantecSpaceUsers";
        }

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtConfig.SecretKey)),
                ValidateIssuer = true,
                ValidIssuer = jwtConfig.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtConfig.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        builder.Services.AddAuthorization();

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
        builder.Services.AddScoped<UserService>();        
        builder.Services.AddScoped<XPService>();        
        builder.Services.AddSingleton<LevelSystem>();        
        // Tilføj authentication services        
        builder.Services.AddScoped<JwtService>();        
        builder.Services.AddScoped<AuthService>();        
        builder.Services.AddScoped<DiscordVerificationService>();
        // Tilføj DBAccess        
        builder.Services.AddScoped<JWTDBAccess>();
        builder.Services.AddScoped<AuthDBAccess>();
        builder.Services.AddScoped<DiscordVerificationDBAccess>();
        builder.Services.AddScoped<DiscordBotDBAccess>();

        // Tilføj cleanup job
        builder.Services.AddHostedService<CleanupJob>();

        // Tilføj logging konfiguration
        builder.Logging.AddConsole();
        builder.Logging.SetMinimumLevel(LogLevel.Information);

        // Tilføj HttpClient til DI container
        builder.Services.AddHttpClient();

        var app = builder.Build();

        // Log at Discord botten starter
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Discord bot starter op...");

        app.MapDefaultEndpoints();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mercantec-Space API v1");
                c.RoutePrefix = "swagger"; // Swagger UI vil være tilgængelig på /swagger
            });
        }

        app.UseHttpsRedirection();

        // Tilføj CORS - skal være før Authentication og Authorization
        app.UseCors("AllowFrontend");

        // Vigtig rækkefølge: Authentication før Authorization
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
