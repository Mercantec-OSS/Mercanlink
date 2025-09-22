namespace Backend.Services;

using Backend.Data;
using Backend.DBAccess;
using Backend.Models;
using Backend.Models.DTOs;
using BCrypt.Net;
using Discord;
using Microsoft.EntityFrameworkCore;

public class AuthService
{

    private readonly AuthDBAccess _authDBAccess;
    private readonly JwtService _jwtService;

    public AuthService(JwtService jwtService, AuthDBAccess authDBAccess)
    {
        _authDBAccess = authDBAccess;
        _jwtService = jwtService;
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        // Find bruger via email eller username
        var websiteUser = await _authDBAccess.Login(request);

        if (websiteUser == null || !BCrypt.Verify(request.Password, websiteUser.Password))
        {
            return null;
        }

        var user = await _authDBAccess.GetWebsiteUser(websiteUser.Id);
        if (user == null)
            return null;

        // Generer tokens
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = await _jwtService.GenerateRefreshTokenAsync(user.WebsiteUserId);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60), // Dette skal matche JWT config
            User = MapToUserDto(user)
        };
    }

    public async Task<User?> GetUserFromWebsiteId(string websiteId)
    {
        var user = await _authDBAccess.GetWebsiteUser(websiteId);
        if (user == null)
            return null;
        return user;
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        // Tjek om email eller username allerede eksisterer
        if (await _authDBAccess.CheckForExistingUser(request))
        {
            throw new InvalidOperationException("Email eller brugernavn er allerede i brug");
        }

        // Tjek om Discord ID allerede er linket
        if (!string.IsNullOrEmpty(request.DiscordId))
        {

            if (await _authDBAccess.CheckForExistingDiscordIdLink(request))
            {
                throw new InvalidOperationException("Discord konto er allerede linket til en anden bruger");
            }
        }

        // Hash password
        var passwordHash = BCrypt.HashPassword(request.Password);

        User user;

        // Opret ny bruger uden Discord
        user = new User
        {
            UserName = request.Username,
            SchoolADUser = new SchoolADUser(),
            DiscordUser = new DiscordUser(),
            WebsiteUser = new WebsiteUser
            {
                UserName = request.Username,
                Email = request.Email,
                Password = passwordHash,
                EmailConfirmed = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        await _authDBAccess.AddUser(user);



        // Generer tokens
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = await _jwtService.GenerateRefreshTokenAsync(user.WebsiteUser.Id);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60),
            User = MapToUserDto(user)
        };
    }

    public async Task<bool> LinkDiscordAsync(string userId, string discordId)
    {
        var user = await _authDBAccess.GetUser(userId);
        if (user == null)
            return false;

        // Tjek om Discord ID allerede er i brug
        var existingDiscordUser = await _authDBAccess.GetDiscordUser(discordId);

        if (existingDiscordUser != null && existingDiscordUser.Id != userId)
        {
            // Hvis der eksisterer en Discord-only bruger (uden email/password), merge med den nuværende bruger
            if (string.IsNullOrEmpty(existingDiscordUser.WebsiteUser.Email) && string.IsNullOrEmpty(existingDiscordUser.WebsiteUser.Password))
            {
                // Kopier Discord data til den nuværende bruger
                user.DiscordUser.DiscordId = existingDiscordUser.DiscordUser.DiscordId;
                user.DiscordUser.GlobalName = existingDiscordUser.DiscordUser.GlobalName;
                user.DiscordUser.Discriminator = existingDiscordUser.DiscordUser.Discriminator;
                user.DiscordUser.AvatarUrl = existingDiscordUser.DiscordUser.AvatarUrl;
                user.DiscordUser.Nickname = existingDiscordUser.DiscordUser.Nickname;
                user.DiscordUser.IsBot = existingDiscordUser.DiscordUser.IsBot;
                user.DiscordUser.PublicFlags = existingDiscordUser.DiscordUser.PublicFlags;
                user.DiscordUser.JoinedAt = existingDiscordUser.DiscordUser.JoinedAt;
                user.DiscordUser.IsBoosting = existingDiscordUser.DiscordUser.IsBoosting;
                user.DiscordUser.Experience = Math.Max(user.DiscordUser.Experience, existingDiscordUser.DiscordUser.Experience);
                user.DiscordUser.Level = Math.Max(user.DiscordUser.Level, existingDiscordUser.DiscordUser.Level);
                user.DiscordUser.UpdatedAt = DateTime.UtcNow;

                // Slet den gamle Discord-only bruger
                await _authDBAccess.DeleteUser(existingDiscordUser);
            }
            else
            {
                return false; // Discord konto er allerede linket til en anden fuld bruger
            }
        }
        else
        {
            user.DiscordUser.DiscordId = discordId;
            user.DiscordUser.UpdatedAt = DateTime.UtcNow;
        }

        await _authDBAccess.UpdateUser(user);
        return true;
    }

    public async Task<AuthResponse?> RefreshTokenAsync(string refreshToken)
    {
        var websiteUser = await _jwtService.ValidateRefreshTokenAsync(refreshToken);
        if (websiteUser == null)
            return null;
        var user = await _authDBAccess.GetWebsiteUser(websiteUser.Id);
        if (user == null)
            return null;

        // Revoke den gamle token
        await _jwtService.RevokeRefreshTokenAsync(refreshToken);

        // Generer nye tokens
        var newAccessToken = _jwtService.GenerateAccessToken(user);
        var newRefreshToken = await _jwtService.GenerateRefreshTokenAsync(user.WebsiteUserId);

        return new AuthResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60),
            User = MapToUserDto(user)
        };
    }

    public async Task<bool> LogoutAsync(string refreshToken)
    {
        await _jwtService.RevokeRefreshTokenAsync(refreshToken);
        return true;
    }

    public async Task<bool> UnlinkDiscordAsync(string userId)
    {
        var user = await _authDBAccess.GetUser(userId);
        if (user == null)
            return false;

        if (string.IsNullOrEmpty(user.DiscordUser.DiscordId))
            return false;
        // Opret ny bruger med kun Discord-information
        var newUser = new User
        {
            UserName = user.DiscordUser.UserName,
            Roles = user.Roles, // Standard rolle
            WebsiteUser = new WebsiteUser(),
            SchoolADUser = new SchoolADUser(),
            DiscordUser = new DiscordUser
            {
                DiscordId = user.DiscordUser.DiscordId,
                UserName = user.DiscordUser.UserName,
                GlobalName = user.DiscordUser.GlobalName ?? string.Empty,
                Discriminator = user.DiscordUser.Discriminator,
                AvatarUrl = user.DiscordUser.AvatarUrl,
                IsBot = user.DiscordUser.IsBot,
                PublicFlags = (int)user.DiscordUser.PublicFlags,
                Nickname = user.DiscordUser.Nickname ?? string.Empty,
                JoinedAt = user.DiscordUser.JoinedAt,
                IsBoosting = user.DiscordUser.IsBoosting,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                // Standardværdier
                Experience = user.DiscordUser.Experience,
                Level = user.DiscordUser.Level
            }
        };

        // Fjern Discord felter
        user.DiscordUser.DiscordId = null;
        user.DiscordUser.UserName = null;
        user.DiscordUser.GlobalName = null;
        user.DiscordUser.Discriminator = null;
        user.DiscordUser.AvatarUrl = null;
        user.DiscordUser.IsBot = null;
        user.DiscordUser.PublicFlags = null;
        user.DiscordUser.Nickname = null;
        user.DiscordUser.PublicFlags = null;
        user.DiscordUser.JoinedAt = null;
        user.DiscordUser.IsBoosting = null;
        user.DiscordUser.Experience = 0;
        user.DiscordUser.Level = 1;
        user.DiscordUser.UpdatedAt = DateTime.UtcNow;

        await _authDBAccess.AddUser(newUser);
        await _authDBAccess.UpdateUser(user);
        return true;
    }

    private static UserDto MapToUserDto(User user) { return new UserDto { UserId = user.Id, Email = user.WebsiteUser.Email, Username = user.UserName, UpdatedAt = user.UpdatedAt, CreatedAt = user.CreatedAt }; }
}