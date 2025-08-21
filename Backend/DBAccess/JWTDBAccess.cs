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

        public async Task<RefreshToken?> GetRefreshToken(string refreshToken)
        {
            var tokenEntity = _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken);
            return await tokenEntity;
        }

        public async Task UpdateRefreshToken(RefreshToken tokenEntity)
        {
            _context.Entry(tokenEntity).State = EntityState.Modified;

            await _context.SaveChangesAsync();
        }

        public async Task<string> AddRefreshToken(RefreshToken tokenEntity)
        {
            _context.RefreshTokens.Add(tokenEntity);

            await _context.SaveChangesAsync();

            return tokenEntity.Token;
        }

        public async Task<RefreshToken?> GetRefreshTokenAndUser(string refreshToken)
        {
            var tokenEntity = _context.RefreshTokens.Include(rt => rt.WebsiteUser).FirstOrDefaultAsync(rt => rt.Token == refreshToken);
            return await tokenEntity;
        }

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
