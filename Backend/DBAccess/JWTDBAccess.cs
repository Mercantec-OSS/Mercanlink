using Backend.Config;
using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Backend.DBAccess
{
    public class JWTDBAccess
    {
        private readonly ApplicationDbContext _context;

        public JWTDBAccess(ApplicationDbContext context)
        {
            _context = context;
        }

        // Gets the refreshtoken that matches the refreshtoken given
        public async Task<RefreshToken?> GetRefreshToken(string refreshToken)
        {
            var tokenEntity = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken);
            return tokenEntity;
        }

        // Updates the refreshtoken
        public async Task UpdateRefreshToken(RefreshToken tokenEntity)
        {
            _context.Entry(tokenEntity).State = EntityState.Modified;

            await _context.SaveChangesAsync();
        }

        // Adds a new refreshtoken
        public async Task<string> AddRefreshToken(RefreshToken tokenEntity)
        {
            _context.RefreshTokens.Add(tokenEntity);

            await _context.SaveChangesAsync();

            return tokenEntity.Token;
        }

        // Gets the refreshtoken and website user that matches the refreshtoken
        public async Task<RefreshToken?> GetRefreshTokenAndUser(string refreshToken)
        {
            var tokenEntity = _context.RefreshTokens.Include(rt => rt.WebsiteUser).FirstOrDefaultAsync(rt => rt.Token == refreshToken);
            return await tokenEntity;
        }

        // Revokes all refreshtokens that matches the website user id
        public async Task RevokeAllRefreshTokens(string websiteUserId)
        {
            var userTokens = await _context
            .RefreshTokens.Where(rt => rt.WebsiteUserId == websiteUserId && !rt.IsRevoked)
            .ToListAsync();

            foreach (var token in userTokens)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }
    }
}
