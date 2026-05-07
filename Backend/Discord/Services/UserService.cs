namespace Backend.DiscordServices.Services;

using System;
using System.Threading.Tasks;
using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Backend.DBAccess;
using global::Discord.WebSocket;
using Microsoft.Extensions.Configuration;

public class UserService
{
    private readonly DiscordBotDBAccess _discordBotDBAccess;
    private readonly ApplicationDbContext _context;
    private readonly HashSet<string> _adminDiscordIds;

    public UserService(DiscordBotDBAccess discordBotDBAccess, ApplicationDbContext context, IConfiguration configuration)
    {
        _discordBotDBAccess = discordBotDBAccess;
        _context = context;
        _adminDiscordIds = ParseAdminDiscordIds(
            Environment.GetEnvironmentVariable("ADMIN_DISCORD_IDS")
            ?? configuration["Admins:DiscordIds"]
            ?? string.Empty
        );
    }

    public async Task<DiscordUser?> GetUserByDiscordIdAsync(string discordId)
    {
        return await _discordBotDBAccess.GetDiscordUser(discordId);
    }

    public async Task<DiscordUser> CreateDiscordUserAsync(SocketGuildUser guildUser)
    {
        // Tjek om brugeren allerede eksisterer
        var existingUser = await _discordBotDBAccess.GetDiscordUser(guildUser.Id.ToString());


        if (existingUser != null)
        {
            await EnsureAdminRoleIfConfiguredAsync(guildUser.Id.ToString());
            return existingUser; // Bruger findes allerede
        }

        // Sikrer at alle DateTime-værdier er i UTC
        var joinedAtUtc = guildUser.JoinedAt.HasValue
            ? DateTime.SpecifyKind(guildUser.JoinedAt.Value.DateTime, DateTimeKind.Utc)
            : DateTime.UtcNow;

        var nowUtc = DateTime.UtcNow;

        // Opret ny bruger med kun Discord-information
        var roles = new List<string> { UserRole.Student.ToString() };
        if (IsAdminDiscordId(guildUser.Id.ToString()))
        {
            roles.Add(UserRole.Admin.ToString());
        }

        var newUser = new User
        {
            UserName = guildUser.Username,
            Roles = roles, // Standard rolle + evt. Admin
            WebsiteUser = new WebsiteUser(),
            SchoolADUser = new SchoolADUser(),
            DiscordUser = new DiscordUser
            {
                DiscordId = guildUser.Id.ToString(),
                UserName = guildUser.Username,
                GlobalName = guildUser.GlobalName ?? string.Empty,
                Discriminator = guildUser.Discriminator,
                AvatarUrl = guildUser.GetAvatarUrl() ?? guildUser.GetDefaultAvatarUrl(),
                IsBot = guildUser.IsBot,
                PublicFlags = (int)guildUser.PublicFlags,
                Nickname = guildUser.Nickname ?? string.Empty,
                JoinedAt = joinedAtUtc,
                IsBoosting = guildUser.PremiumSince.HasValue,
                CreatedAt = nowUtc,
                UpdatedAt = nowUtc,
                // Standardværdier
                Experience = 0,
                Level = 1
            }
        };


        await _discordBotDBAccess.AddUser(newUser);

        return newUser.DiscordUser;
    }

    private bool IsAdminDiscordId(string discordId)
    {
        return _adminDiscordIds.Contains(discordId.Trim());
    }

    private async Task EnsureAdminRoleIfConfiguredAsync(string discordId)
    {
        if (!IsAdminDiscordId(discordId))
        {
            return;
        }

        var user = await _context.Users
            .Include(u => u.DiscordUser)
            .FirstOrDefaultAsync(u => u.DiscordUser != null && u.DiscordUser.DiscordId == discordId);

        if (user == null)
        {
            return;
        }

        user.Roles ??= new List<string>();
        if (!user.Roles.Contains(UserRole.Admin.ToString()))
        {
            user.Roles.Add(UserRole.Admin.ToString());
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    private static HashSet<string> ParseAdminDiscordIds(string raw)
    {
        var set = new HashSet<string>(StringComparer.Ordinal);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return set;
        }

        var parts = raw.Split(new[] { ',', ';', ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in parts)
        {
            var trimmed = part.Trim();
            if (!string.IsNullOrEmpty(trimmed))
            {
                set.Add(trimmed);
            }
        }
        return set;
    }
}
