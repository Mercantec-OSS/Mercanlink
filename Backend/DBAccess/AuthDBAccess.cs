using Backend.Data;
using Backend.Models;
using Backend.Models.DTOs;
using Discord.Net;
using Microsoft.EntityFrameworkCore;

namespace Backend.DBAccess
{
    public class AuthDBAccess
    {
        private readonly ApplicationDbContext _context;

        public AuthDBAccess(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<WebsiteUser?> Login(LoginRequest request)
        {
            var user = await _context.WebsiteUsers
            .FirstOrDefaultAsync(u =>
                u.Email == request.EmailOrUsername ||
                u.UserName == request.EmailOrUsername);

            return user;
        }

        public async Task<bool> CheckForExistingUser(RegisterRequest request)
        {
            var existingUser = await _context.WebsiteUsers
            .FirstOrDefaultAsync(u =>
                u.Email == request.Email ||
                u.UserName == request.Username);

            if (existingUser != null) { return true; }
            return false;
        }

        public async Task<bool> CheckForExistingDiscordIdLink(RegisterRequest request)
        {
            var existingUser = await _context.Users.Include(u => u.WebsiteUser)
                .Include(u => u.DiscordUser)
                .FirstOrDefaultAsync(u => u.DiscordUser.DiscordId == request.DiscordId && !string.IsNullOrEmpty(u.WebsiteUser.Email));

            if (existingUser != null) { return true; }
            return false;
        }

        public async Task<User?> GetDiscordUser(string discordId)
        {
            var discordUser = await _context.Users.Include(u => u.DiscordUser).Include(u => u.WebsiteUser)
                .FirstOrDefaultAsync(u => u.DiscordUser.DiscordId == discordId);

            return discordUser;
        }

        public async Task<User?> GetWebsiteUser(string WebsiteUserID)
        {
            var discordUser = await _context.Users.Include(u => u.DiscordUser).Include(u => u.WebsiteUser)
                .FirstOrDefaultAsync(u => u.WebsiteUserId == WebsiteUserID);

            return discordUser;
        }

        public async Task UpdateUser(User user)
        {
            _context.Entry(user).State = EntityState.Modified;

            await _context.SaveChangesAsync();
        }

        public async Task AddUser(User user)
        {
            _context.Users.Add(user);

            await _context.SaveChangesAsync();
        }

        public async Task<User?> GetUser(string userId)
        {
            var user = await _context.Users.Include(u => u.WebsiteUser).Include(u => u.DiscordUser).FirstOrDefaultAsync(u => u.Id == userId);

            return user;
        }

        public async Task DeleteUser(User user)
        {
            _context.Users.Remove(user);

            await _context.SaveChangesAsync();
        }
    }
}
