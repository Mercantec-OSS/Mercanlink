using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

public enum EventType
{
    Lan = 0,
    Workshop = 1,
    Talk = 2,
    Hackathon = 3,
    Other = 99
}

public enum EventStatus
{
    Draft = 0,
    Published = 1,
    Cancelled = 2
}

public class Event : Common
{
    [MaxLength(160)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(160)]
    public string Slug { get; set; } = string.Empty;

    [MaxLength(8000)]
    public string Description { get; set; } = string.Empty;

    public EventType Type { get; set; } = EventType.Other;

    public EventStatus Status { get; set; } = EventStatus.Draft;

    public DateTime StartsAt { get; set; }

    public DateTime EndsAt { get; set; }

    [MaxLength(200)]
    public string Location { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? LocationUrl { get; set; }

    [MaxLength(500)]
    public string? BannerImageUrl { get; set; }

    public int? Capacity { get; set; }

    public DateTime? RegistrationDeadline { get; set; }

    // LAN
    public bool? BringOwnPc { get; set; }

    // Talk
    [MaxLength(160)]
    public string? SpeakerName { get; set; }

    // Workshop
    [MaxLength(2000)]
    public string? Prerequisites { get; set; }

    // Hackathon
    public int? TeamSize { get; set; }

    [MaxLength(120)]
    public string? CreatedByUserId { get; set; }

    public User? CreatedByUser { get; set; }

    // Discord-integration metadata
    public ulong? DiscordChannelId { get; set; }

    public ulong? DiscordAnnouncementMessageId { get; set; }

    public ulong? DiscordScheduledEventId { get; set; }

    public DateTime? AnnouncementSentAt { get; set; }

    public bool ReminderDayBeforeSent { get; set; }

    public bool ReminderHourBeforeSent { get; set; }

    public List<EventRegistration> Registrations { get; set; } = new();
}
