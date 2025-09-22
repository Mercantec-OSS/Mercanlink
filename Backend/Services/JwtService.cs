namespace Backend.Services;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Backend.Config;
using Backend.Data;
using Backend.DBAccess;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

public class JwtService
{
    private readonly JwtConfig _jwtConfig;
    private readonly JWTDBAccess _jwtDBAccess;

    public JwtService(IOptions<JwtConfig> jwtConfig, JWTDBAccess jwtDBAccess)
    {
        _jwtConfig = jwtConfig.Value;
        _jwtDBAccess = jwtDBAccess;
    }

    public string GenerateAccessToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtConfig.SecretKey);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.WebsiteUser.Email),
            new(ClaimTypes.Name, user.UserName)
        };

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
                SecurityAlgorithms.HmacSha256Signature
            )
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public async Task<string> GenerateRefreshTokenAsync(string websiteUserId)
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
            WebsiteUserId = websiteUserId,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtConfig.RefreshTokenExpiryDays),
            CreatedAt = DateTime.UtcNow
        };

        return await _jwtDBAccess.AddRefreshToken(tokenEntity);
    }

    public async Task<WebsiteUser?> ValidateRefreshTokenAsync(string refreshToken)
    {
        var tokenEntity = await _jwtDBAccess.GetRefreshTokenAndUser(refreshToken);

        if (
            tokenEntity == null
            || tokenEntity.IsRevoked
            || tokenEntity.ExpiresAt <= DateTime.UtcNow
        )
        {
            return null;
        }

        return tokenEntity.WebsiteUser;
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken, string? replacedByToken = null)
    {
        var tokenEntity = await _jwtDBAccess.GetRefreshToken(refreshToken);

        if (tokenEntity != null)
        {
            tokenEntity.IsRevoked = true;
            tokenEntity.RevokedAt = DateTime.UtcNow;
            tokenEntity.ReplacedByToken = replacedByToken;
            await _jwtDBAccess.UpdateRefreshToken(tokenEntity);
        }
    }

    public async Task RevokeAllUserTokensAsync(string websiteUserId)
    {
        await _jwtDBAccess.RevokeAllRefreshTokens(websiteUserId);
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
