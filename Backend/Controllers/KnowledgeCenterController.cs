using Backend.Data;
using Backend.Models;
using Backend.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class KnowledgeCenterController : ControllerBase
{
    private readonly DiscordBotService _discordBotService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<KnowledgeCenterController> _logger;

    public KnowledgeCenterController(
        DiscordBotService discordBotService,
        ApplicationDbContext context,
        ILogger<KnowledgeCenterController> logger
    )
    {
        _discordBotService = discordBotService;
        _context = context;
        _logger = logger;
    }

    [HttpPost]
    [Authorize]
    [EnableRateLimiting("KnowledgeCenterSubmit")]
    public async Task<ActionResult<KnowledgeSubmissionResponse>> AddNewPostForApproval([FromBody] CreateKnowledgeSubmissionRequest request)
    {
        if (!request.HasValidType())
        {
            return BadRequest(new { message = "Ugyldig materialetype." });
        }

        var currentUser = await ResolveCurrentUserAsync();
        if (currentUser == null)
        {
            currentUser = await ProvisionUserFromClaimsAsync();
            if (currentUser == null)
            {
                return Unauthorized(new { message = "Kunne ikke finde eller oprette den indloggede bruger i systemet." });
            }
        }

        var submission = new KnowledgeSubmission
        {
            Type = request.Type.Trim().ToLowerInvariant(),
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            LinkToPost = request.LinkToPost?.Trim() ?? string.Empty,
            UserId = currentUser.Id,
            DiscordId = currentUser.DiscordUser?.DiscordId ?? string.Empty,
            AuthorName = ResolveAuthorName(currentUser),
            Status = KnowledgeSubmissionStatus.Pending
        };

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            _context.KnowledgeSubmissions.Add(submission);
            await _context.SaveChangesAsync();

            submission.ModMessageId = await _discordBotService.SendSubmissionForApprovalAsync(submission);
            submission.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            _logger.LogInformation(
                "Knowledge submission {SubmissionId} oprettet af bruger {UserId} og sendt til moderation",
                submission.Id,
                currentUser.Id
            );

            return Ok(MapToResponse(submission));
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Fejl under oprettelse af knowledge submission for bruger {UserId}", currentUser.Id);
            return StatusCode(500, new { message = "Der opstod en fejl under indsendelse til moderation." });
        }
    }

    [HttpGet("pending")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult<List<KnowledgeSubmissionResponse>>> GetPendingSubmissions()
    {
        var submissions = await _context.KnowledgeSubmissions
            .AsNoTracking()
            .Where(s => s.Status == KnowledgeSubmissionStatus.Pending)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        return Ok(submissions.Select(MapToResponse).ToList());
    }

    [HttpPatch("{submissionId}/approve")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult<KnowledgeSubmissionResponse>> ApproveSubmission(string submissionId)
    {
        var submission = await _context.KnowledgeSubmissions.FirstOrDefaultAsync(s => s.Id == submissionId);
        if (submission == null)
        {
            return NotFound(new { message = "Submission blev ikke fundet." });
        }

        if (submission.Status != KnowledgeSubmissionStatus.Pending)
        {
            return Conflict(new { message = "Submission er allerede behandlet." });
        }

        try
        {
            submission.Status = KnowledgeSubmissionStatus.Approved;
            submission.ReviewedByUserId = ResolveReviewerId();
            submission.ReviewedAt = DateTime.UtcNow;
            submission.RejectionReason = null;
            submission.PublishedMessageId = await _discordBotService.PublishApprovedSubmissionAsync(submission);
            submission.PublishedToDiscordAt = DateTime.UtcNow;
            submission.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation(
                "Knowledge submission {SubmissionId} godkendt af {ReviewerId}",
                submissionId,
                submission.ReviewedByUserId
            );
            return Ok(MapToResponse(submission));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fejl under godkendelse af submission {SubmissionId}", submissionId);
            return StatusCode(500, new { message = "Der opstod en fejl under godkendelse." });
        }
    }

    [HttpPatch("{submissionId}/reject")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult<KnowledgeSubmissionResponse>> RejectSubmission(
        string submissionId,
        [FromBody] KnowledgeSubmissionReviewRequest request
    )
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            return BadRequest(new { message = "Afvisningsgrund er påkrævet." });
        }

        var submission = await _context.KnowledgeSubmissions.FirstOrDefaultAsync(s => s.Id == submissionId);
        if (submission == null)
        {
            return NotFound(new { message = "Submission blev ikke fundet." });
        }

        if (submission.Status != KnowledgeSubmissionStatus.Pending)
        {
            return Conflict(new { message = "Submission er allerede behandlet." });
        }

        submission.Status = KnowledgeSubmissionStatus.Rejected;
        submission.ReviewedByUserId = ResolveReviewerId();
        submission.ReviewedAt = DateTime.UtcNow;
        submission.RejectionReason = request.Reason.Trim();
        submission.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger.LogInformation(
            "Knowledge submission {SubmissionId} afvist af {ReviewerId}",
            submissionId,
            submission.ReviewedByUserId
        );
        return Ok(MapToResponse(submission));
    }

    private async Task<User?> ResolveCurrentUserAsync()
    {
        var currentUserId = GetCurrentUserId();
        var email = GetClaimValue(ClaimTypes.Email, "email");
        var username = GetClaimValue("preferred_username", ClaimTypes.Name, "name");

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

    private string ResolveAuthorName(User user)
    {
        if (!string.IsNullOrWhiteSpace(user.DiscordUser?.GlobalName))
        {
            return user.DiscordUser.GlobalName;
        }

        if (!string.IsNullOrWhiteSpace(user.UserName))
        {
            return user.UserName;
        }

        if (!string.IsNullOrWhiteSpace(user.WebsiteUser?.UserName))
        {
            return user.WebsiteUser.UserName;
        }

        return "Ukendt bruger";
    }

    private async Task<User?> ProvisionUserFromClaimsAsync()
    {
        var subject = GetCurrentUserId();
        var email = GetClaimValue(ClaimTypes.Email, "email");
        var username = GetClaimValue("preferred_username", ClaimTypes.Name, "name");
        var fallbackUserName = username
            ?? (!string.IsNullOrWhiteSpace(email) ? email.Split('@')[0] : null)
            ?? "user";

        // Opretter en minimal User + relationer, så features kan bruges med det samme.
        // DiscordId linkes separat via eksisterende verification-flow.
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

    private string ResolveReviewerId()
    {
        return User.FindFirst("sub")?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? "unknown-reviewer";
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    private string? GetClaimValue(params string[] claimTypes)
    {
        foreach (var claimType in claimTypes)
        {
            var value = User.FindFirst(claimType)?.Value;
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        return null;
    }

    private static KnowledgeSubmissionResponse MapToResponse(KnowledgeSubmission submission)
    {
        return new KnowledgeSubmissionResponse
        {
            Id = submission.Id,
            Type = submission.Type,
            Title = submission.Title,
            Description = submission.Description,
            LinkToPost = submission.LinkToPost,
            AuthorName = submission.AuthorName,
            DiscordId = submission.DiscordId,
            Status = submission.Status,
            ReviewedByUserId = submission.ReviewedByUserId,
            ReviewedAt = submission.ReviewedAt,
            RejectionReason = submission.RejectionReason,
            ModMessageId = submission.ModMessageId,
            PublishedMessageId = submission.PublishedMessageId,
            PublishedToDiscordAt = submission.PublishedToDiscordAt,
            CreatedAt = submission.CreatedAt,
            UpdatedAt = submission.UpdatedAt
        };
    }
}
