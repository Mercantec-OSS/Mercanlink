using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.DBAccess
{
    public class DiscordVerificationDBAccess
    {
        private readonly ApplicationDbContext _context;

        public DiscordVerificationDBAccess(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> CheckExistingUser(string discordId)
        {
            // Tjek om Discord ID allerede er verificeret
            var existingUser = await _context.Users.Include(u => u.WebsiteUser).Include(u => u.DiscordUser).FirstOrDefaultAsync(u => u.DiscordUser.DiscordId == discordId && !string.IsNullOrEmpty(u.WebsiteUser.Email));
            return existingUser;
        }

        public async Task<bool> RemoveExistingVerifications(string userId, string discordId)
        {
            // Slet eventuelt eksisterende verification for denne bruger og Discord ID
            var existingVerifications = await _context.DiscordVerifications
                .Where(dv => (dv.UserId == userId || dv.DiscordId == discordId) && !dv.IsUsed)
                .ToListAsync();

            if (existingVerifications.Any())
            {
                _context.DiscordVerifications.RemoveRange(existingVerifications);
            }

            return await _context.SaveChangesAsync() >= 1;
        }

        public async Task AddVerification(DiscordVerification verification)
        {
            _context.DiscordVerifications.Add(verification);

            await _context.SaveChangesAsync();
        }

        public async Task<DiscordVerification?> CheckVerificationCode(string userId, string discordId, string code)
        {
            var verification = await _context.DiscordVerifications
                .FirstOrDefaultAsync(dv =>
                    dv.UserId == userId &&
                    dv.DiscordId == discordId &&
                    dv.VerificationCode == code &&
                    !dv.IsUsed &&
                    dv.ExpiresAt > DateTime.UtcNow);
            return verification;
        }

        public async Task UpdateVerificationCode(DiscordVerification verification)
        {
            _context.Entry(verification).State = EntityState.Modified;

            await _context.SaveChangesAsync();
        }

        public async Task<DiscordVerification?> GetActiveVerification(string userId, string discordId)
        {
            return await _context.DiscordVerifications
            .FirstOrDefaultAsync(dv =>
                dv.UserId == userId &&
                dv.DiscordId == discordId &&
                !dv.IsUsed &&
                dv.ExpiresAt > DateTime.UtcNow);
        }

        public async Task<int> CleanupExpiredVerifications()
        {
            var expired = await _context.DiscordVerifications
            .Where(dv => dv.ExpiresAt <= DateTime.UtcNow || dv.CreatedAt < DateTime.UtcNow.AddDays(-1))
            .ToListAsync();

            if (expired.Any())
            {
                _context.DiscordVerifications.RemoveRange(expired);
                await _context.SaveChangesAsync();
            }
            return expired.Count;
        }
    }
}
