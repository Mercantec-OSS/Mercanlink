using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Backend.Services;

public class AuthenticatedUserService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AuthenticatedUserService> _logger;

    public AuthenticatedUserService(ApplicationDbContext context, ILogger<AuthenticatedUserService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<User?> ResolveCurrentUserAsync(ClaimsPrincipal principal)
    {
        var currentUserId = GetCurrentUserId(principal);
        var email = GetClaimValue(principal, ClaimTypes.Email, "email");
        var username = GetClaimValue(principal, "preferred_username", ClaimTypes.Name, "name");

        if (string.IsNullOrWhiteSpace(currentUserId)
            && string.IsNullOrWhiteSpace(email)
            && string.IsNullOrWhiteSpace(username))
        {
            return null;
        }

        return await _context.Users
            .Include(u => u.DiscordUser)
            .Include(u => u.WebsiteUser)
            .Include(u => u.SchoolADUser)
            .FirstOrDefaultAsync(u =>
                (!string.IsNullOrWhiteSpace(currentUserId) && (
                    u.Id == currentUserId
                    || u.WebsiteUserId == currentUserId
                    || u.DiscordUserId == currentUserId
                    || u.DiscordUser.DiscordId == currentUserId
                ))
                || (!string.IsNullOrWhiteSpace(email) && (
                    u.WebsiteUser.Email == email
                ))
                || (!string.IsNullOrWhiteSpace(username) && (
                    u.UserName == username
                    || u.WebsiteUser.UserName == username
                    || u.DiscordUser.UserName == username
                    || u.DiscordUser.GlobalName == username
                ))
            );
    }

    public async Task<User?> ProvisionUserFromClaimsAsync(ClaimsPrincipal principal)
    {
        var subject = GetCurrentUserId(principal);
        var email = GetClaimValue(principal, ClaimTypes.Email, "email");
        var username = GetClaimValue(principal, "preferred_username", ClaimTypes.Name, "name");
        var fallbackUserName = username
            ?? (!string.IsNullOrWhiteSpace(email) ? email.Split('@')[0] : null)
            ?? "user";

        var websiteUser = new WebsiteUser
        {
            UserName = fallbackUserName,
            Email = email ?? "",
            Password = "",
            EmailConfirmed = !string.IsNullOrWhiteSpace(email),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var discordUser = new DiscordUser
        {
            UserName = fallbackUserName,
            GlobalName = fallbackUserName,
            DiscordId = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var schoolAdUser = new SchoolADUser
        {
            UserName = fallbackUserName,
            StudentId = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var user = new User
        {
            UserName = fallbackUserName,
            Roles = new List<string> { "Student" },
            WebsiteUserId = websiteUser.Id,
            DiscordUserId = discordUser.Id,
            SchoolADUserId = schoolAdUser.Id,
            WebsiteUser = websiteUser,
            DiscordUser = discordUser,
            SchoolADUser = schoolAdUser,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            _context.WebsiteUsers.Add(websiteUser);
            _context.DiscordUsers.Add(discordUser);
            _context.SchoolADUsers.Add(schoolAdUser);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation(
                "Provisionerede ny bruger fra claims (sub={Sub}, email={Email}, username={Username}) -> userId={UserId}",
                subject,
                email,
                username,
                user.Id
            );

            return user;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Fejl under provisioning af bruger fra claims (sub={Sub})", subject);
            return null;
        }
    }

    public static string ResolveDisplayName(User user)
    {
        if (!string.IsNullOrWhiteSpace(user.DiscordUser?.GlobalName))
            return user.DiscordUser.GlobalName;

        if (!string.IsNullOrWhiteSpace(user.UserName))
            return user.UserName;

        if (!string.IsNullOrWhiteSpace(user.WebsiteUser?.UserName))
            return user.WebsiteUser.UserName!;

        return "Ukendt bruger";
    }

    public static string ResolveReviewerId(ClaimsPrincipal principal)
    {
        return principal.FindFirst("sub")?.Value
            ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? "unknown-reviewer";
    }

    public static string? GetCurrentUserId(ClaimsPrincipal principal)
    {
        return principal.FindFirst("sub")?.Value ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    public static string? GetClaimValue(ClaimsPrincipal principal, params string[] claimTypes)
    {
        foreach (var claimType in claimTypes)
        {
            var value = principal.FindFirst(claimType)?.Value;
            if (!string.IsNullOrWhiteSpace(value))
                return value.Trim();
        }

        return null;
    }
}
