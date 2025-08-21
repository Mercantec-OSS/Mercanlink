namespace Backend.DiscordServices.Services;

using System;
using System.Threading.Tasks;
using Backend.Data;
using Backend.Models;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Backend.DBAccess;

public class UserService
{
    private readonly DiscordBotDBAccess _discordBotDBAccess;

    public UserService(DiscordBotDBAccess discordBotDBAccess)
    {
        _discordBotDBAccess = discordBotDBAccess;
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
            return existingUser; // Bruger findes allerede
        }

        // Sikrer at alle DateTime-værdier er i UTC
        var joinedAtUtc = guildUser.JoinedAt.HasValue
            ? DateTime.SpecifyKind(guildUser.JoinedAt.Value.DateTime, DateTimeKind.Utc)
            : DateTime.UtcNow;

        var nowUtc = DateTime.UtcNow;

        // Opret ny bruger med kun Discord-information
        var newUser = new User
        {
            UserName = guildUser.Username,
            Roles = new List<string> { UserRole.Student.ToString() }, // Standard rolle
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
                IsActive = true,
                Experience = 0,
                Level = 1
            }
        };


        await _discordBotDBAccess.AddUser(newUser);

        return newUser.DiscordUser;
    }
}
