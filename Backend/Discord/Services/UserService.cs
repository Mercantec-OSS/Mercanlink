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

    public async Task<User> CreateOrUpdateUserAsync(SocketGuildUser guildUser)
    {
        // Søg efter eksisterende bruger
        var existingUser = await _discordBotDBAccess.GetUser(guildUser.Id.ToString());

        if (existingUser != null)
        {
            // Opdater eksisterende bruger
            existingUser.Username = guildUser.Username;
            existingUser.GlobalName = guildUser.GlobalName ?? string.Empty;
            existingUser.Discriminator = guildUser.Discriminator;
            existingUser.AvatarUrl = guildUser.GetAvatarUrl() ?? guildUser.GetDefaultAvatarUrl();
            existingUser.IsBot = guildUser.IsBot;
            existingUser.PublicFlags = (int)guildUser.PublicFlags;
            existingUser.Nickname = guildUser.Nickname ?? string.Empty;

            // Sikrer at DateTime er i UTC
            existingUser.JoinedAt = guildUser.JoinedAt.HasValue
                ? DateTime.SpecifyKind(guildUser.JoinedAt.Value.DateTime, DateTimeKind.Utc)
                : DateTime.UtcNow;

            existingUser.IsBoosting = guildUser.PremiumSince.HasValue;
            existingUser.LastUpdated = DateTime.UtcNow;

            await _discordBotDBAccess.UpdateUser(existingUser);

            return existingUser;
        }
        else
        {
            // Sikrer at alle DateTime-værdier er i UTC
            var joinedAtUtc = guildUser.JoinedAt.HasValue
                ? DateTime.SpecifyKind(guildUser.JoinedAt.Value.DateTime, DateTimeKind.Utc)
                : DateTime.UtcNow;

            var nowUtc = DateTime.UtcNow;

            // Opret ny bruger
            var newUser = new User
            {
                DiscordId = guildUser.Id.ToString(),
                Username = guildUser.Username,
                GlobalName = guildUser.GlobalName ?? string.Empty,
                Discriminator = guildUser.Discriminator,
                AvatarUrl = guildUser.GetAvatarUrl() ?? guildUser.GetDefaultAvatarUrl(),
                IsBot = guildUser.IsBot,
                PublicFlags = (int)guildUser.PublicFlags,
                Nickname = guildUser.Nickname ?? string.Empty,
                JoinedAt = joinedAtUtc,
                IsBoosting = guildUser.PremiumSince.HasValue,
                CreatedAt = nowUtc,
                LastUpdated = nowUtc
            };

            await _discordBotDBAccess.AddUser(newUser);

            return newUser;
        }
    }

    public async Task<User?> GetUserByDiscordIdAsync(string discordId)
    {
        return await _discordBotDBAccess.GetUser(discordId);
    }

    public async Task<User> CreateDiscordUserAsync(SocketGuildUser guildUser)
    {
        // Tjek om brugeren allerede eksisterer
        var existingUser = await _discordBotDBAccess.GetUser(guildUser.Id.ToString());


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
            // Discord-information
            DiscordId = guildUser.Id.ToString(),
            Username = guildUser.Username,
            GlobalName = guildUser.GlobalName ?? string.Empty,
            Discriminator = guildUser.Discriminator,
            AvatarUrl = guildUser.GetAvatarUrl() ?? guildUser.GetDefaultAvatarUrl(),
            IsBot = guildUser.IsBot,
            PublicFlags = (int)guildUser.PublicFlags,
            Nickname = guildUser.Nickname ?? string.Empty,
            JoinedAt = joinedAtUtc,
            IsBoosting = guildUser.PremiumSince.HasValue,

            // Tomme felter (vil blive udfyldt senere ved registrering)
            Email = string.Empty,
            PasswordHash = string.Empty,
            EmailConfirmed = false,

            // Standardværdier
            CreatedAt = nowUtc,
            LastUpdated = nowUtc,
            IsActive = true,
            Experience = 0,
            Level = 1,
            Roles = new List<string> { UserRole.Student.ToString() } // Standard rolle
        };

        await _discordBotDBAccess.AddUser(newUser);

        return newUser;
    }
}
