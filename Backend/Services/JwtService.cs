namespace Backend.Services;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Backend.Config;
using Backend.Data;
using Backend.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;

public class JwtService
{
    private readonly JwtConfig _jwtConfig;
    private readonly ApplicationDbContext _context;

    public JwtService(IOptions<JwtConfig> jwtConfig, ApplicationDbContext context)
    {
        _jwtConfig = jwtConfig.Value;
        _context = context;
    }

        public string GenerateAccessToken(User user)    {        var tokenHandler = new JwtSecurityTokenHandler();        var key = Encoding.ASCII.GetBytes(_jwtConfig.SecretKey);        var claims = new List<Claim>        {            new(ClaimTypes.NameIdentifier, user.Id),            new(ClaimTypes.Email, user.Email),            new(ClaimTypes.Name, user.Username),            new("discord_id", user.DiscordId ?? string.Empty),            new("level", user.Level.ToString()),            new("experience", user.Experience.ToString())        };

        // Tilføj roller som claims
        foreach (var role in user.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtConfig.ExpiryMinutes),
            Issuer = _jwtConfig.Issuer,
            Audience = _jwtConfig.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public async Task<string> GenerateRefreshTokenAsync(string userId)
    {
        // Generer et sikkert random token
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        var refreshToken = Convert.ToBase64String(randomNumber);

        // Gem refresh token i databasen
        var tokenEntity = new RefreshToken
        {
            Token = refreshToken,
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtConfig.RefreshTokenExpiryDays),
            CreatedAt = DateTime.UtcNow
        };

        _context.RefreshTokens.Add(tokenEntity);
        await _context.SaveChangesAsync();

        return refreshToken;
    }

    public async Task<User?> ValidateRefreshTokenAsync(string refreshToken)
    {
        var tokenEntity = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (tokenEntity == null || 
            tokenEntity.IsRevoked || 
            tokenEntity.ExpiresAt <= DateTime.UtcNow)
        {
            return null;
        }

        return tokenEntity.User;
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken, string? replacedByToken = null)
    {
        var tokenEntity = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (tokenEntity != null)
        {
            tokenEntity.IsRevoked = true;
            tokenEntity.RevokedAt = DateTime.UtcNow;
            tokenEntity.ReplacedByToken = replacedByToken;
            await _context.SaveChangesAsync();
        }
    }

    public async Task RevokeAllUserTokensAsync(string userId)
    {
        var userTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync();

        foreach (var token in userTokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtConfig.SecretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtConfig.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtConfig.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            return principal;
        }
        catch
        {
            return null;
        }
    }
} 