using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

public enum KnowledgeSubmissionStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}

public class KnowledgeSubmission : Common
{
    [MaxLength(60)]
    public string Type { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(500)]
    public string LinkToPost { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    [MaxLength(50)]
    public string DiscordId { get; set; } = string.Empty;

    [MaxLength(120)]
    public string AuthorName { get; set; } = string.Empty;

    public KnowledgeSubmissionStatus Status { get; set; } = KnowledgeSubmissionStatus.Pending;

    [MaxLength(120)]
    public string? ReviewedByUserId { get; set; }

    public DateTime? ReviewedAt { get; set; }

    [MaxLength(800)]
    public string? RejectionReason { get; set; }

    public ulong? ModMessageId { get; set; }

    public ulong? PublishedMessageId { get; set; }

    public DateTime? PublishedToDiscordAt { get; set; }

    public User? User { get; set; }
}
