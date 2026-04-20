using Backend.Data;
using Backend.Models;
using Backend.Models.DTOs;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class KnowledgeCenterController : ControllerBase
{
    private readonly DiscordBotService _discordBotService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<KnowledgeCenterController> _logger;
    private readonly AuthenticatedUserService _authenticatedUserService;

    public KnowledgeCenterController(
        DiscordBotService discordBotService,
        ApplicationDbContext context,
        ILogger<KnowledgeCenterController> logger,
        AuthenticatedUserService authenticatedUserService
    )
    {
        _discordBotService = discordBotService;
        _context = context;
        _logger = logger;
        _authenticatedUserService = authenticatedUserService;
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

        var currentUser = await _authenticatedUserService.ResolveCurrentUserAsync(User);
        if (currentUser == null)
        {
            currentUser = await _authenticatedUserService.ProvisionUserFromClaimsAsync(User);
            if (currentUser == null)
            {
                return Unauthorized(new { message = "Kunne ikke finde eller oprette den indloggede bruger i systemet." });
            }
        }

        var discordIdForTag = ResolveSubmissionDiscordId(request.DiscordId, currentUser);
        if (discordIdForTag is null)
        {
            return BadRequest(new { message = "Ugyldigt Discord bruger-ID. Brug kun det numeriske ID (snowflake), fx fra Discord → Indstillinger → Avanceret → udviklertilstand." });
        }

        var submission = new KnowledgeSubmission
        {
            Type = request.Type.Trim().ToLowerInvariant(),
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            LinkToPost = request.LinkToPost?.Trim() ?? string.Empty,
            UserId = currentUser.Id,
            DiscordId = discordIdForTag,
            AuthorName = AuthenticatedUserService.ResolveDisplayName(currentUser),
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
            submission.ReviewedByUserId = AuthenticatedUserService.ResolveReviewerId(User);
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
        submission.ReviewedByUserId = AuthenticatedUserService.ResolveReviewerId(User);
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

    /// <summary>
    /// Tom streng fra request = brug linkede Discord. Ellers skal input være et gyldigt snowflake.
    /// </summary>
    private static string? ResolveSubmissionDiscordId(string? requestDiscordId, User currentUser)
    {
        var trimmed = requestDiscordId?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(trimmed))
        {
            return currentUser.DiscordUser?.DiscordId?.Trim() ?? string.Empty;
        }

        if (!ulong.TryParse(trimmed, out _) || trimmed.Length is < 17 or > 22)
        {
            return null;
        }

        return trimmed;
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
