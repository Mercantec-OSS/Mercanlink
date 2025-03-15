namespace Backend.DiscordServices.Services;

using System;
using System.Threading.Tasks;
using Backend.Data;
using Backend.Models;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

public class UserService
{
    private readonly ApplicationDbContext _context;

    public UserService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User> CreateOrUpdateUserAsync(SocketGuildUser guildUser)
    {
        // Søg efter eksisterende bruger
        var existingUser = await _context.Users.FirstOrDefaultAsync(u =>
            u.DiscordId == guildUser.Id.ToString()
        );

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
            existingUser.JoinedAt = guildUser.JoinedAt?.DateTime ?? DateTime.UtcNow;
            existingUser.IsBoosting = guildUser.PremiumSince.HasValue;
            existingUser.LastUpdated = DateTime.UtcNow;

            _context.Users.Update(existingUser);
            await _context.SaveChangesAsync();

            return existingUser;
        }
        else
        {
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
                JoinedAt = guildUser.JoinedAt?.DateTime ?? DateTime.UtcNow,
                IsBoosting = guildUser.PremiumSince.HasValue,
                CreatedAt = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow
            };

            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();

            return newUser;
        }
    }

    public async Task<User> GetUserByDiscordIdAsync(string discordId)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.DiscordId == discordId);
    }
}
