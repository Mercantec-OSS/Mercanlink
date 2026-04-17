using Backend.Models;
using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTOs;

public class CreateKnowledgeSubmissionRequest
{
    private static readonly string[] AllowedTypes = new[] { "blog-post", "video", "artikel", "andet" };

    [Required(ErrorMessage = "Materialetype er påkrævet.")]
    [StringLength(60)]
    public string Type { get; set; } = string.Empty;

    [Required(ErrorMessage = "Titel er påkrævet.")]
    [StringLength(200, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Beskrivelse er påkrævet.")]
    [StringLength(2000, MinimumLength = 10)]
    public string Description { get; set; } = string.Empty;

    [StringLength(500)]
    [Url(ErrorMessage = "Link skal være en gyldig URL.")]
    public string? LinkToPost { get; set; }

    public bool HasValidType()
    {
        return AllowedTypes.Contains(Type?.Trim(), StringComparer.OrdinalIgnoreCase);
    }
}

public class KnowledgeSubmissionReviewRequest
{
    [StringLength(800)]
    public string? Reason { get; set; }
}

public class KnowledgeSubmissionResponse
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string LinkToPost { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string DiscordId { get; set; } = string.Empty;
    public KnowledgeSubmissionStatus Status { get; set; }
    public string? ReviewedByUserId { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? RejectionReason { get; set; }
    public ulong? ModMessageId { get; set; }
    public ulong? PublishedMessageId { get; set; }
    public DateTime? PublishedToDiscordAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
