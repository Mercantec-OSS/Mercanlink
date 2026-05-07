using System.ComponentModel.DataAnnotations;
using Backend.Models;

namespace Backend.Models.DTOs;

public class EventListItemDto
{
    public string Id { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? BannerImageUrl { get; set; }
    public int? Capacity { get; set; }
    public int RegistrationCount { get; set; }
    public DateTime? RegistrationDeadline { get; set; }
}

public class EventDetailDto : EventListItemDto
{
    public string Description { get; set; } = string.Empty;
    public string? LocationUrl { get; set; }
    public bool? BringOwnPc { get; set; }
    public string? SpeakerName { get; set; }
    public string? Prerequisites { get; set; }
    public int? TeamSize { get; set; }
    public bool RegistrationOpen { get; set; }
    public bool IsFull { get; set; }
}

public class EventRegistrationDto
{
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; }
    public string Source { get; set; } = "Web";
}

public class CreateEventRequest
{
    [Required, MaxLength(160)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(160)]
    public string? Slug { get; set; }

    [Required, MaxLength(8000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public EventType Type { get; set; } = EventType.Other;

    [Required]
    public DateTime StartsAt { get; set; }

    [Required]
    public DateTime EndsAt { get; set; }

    [Required, MaxLength(200)]
    public string Location { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? LocationUrl { get; set; }

    [MaxLength(500)]
    public string? BannerImageUrl { get; set; }

    [Range(1, 10000)]
    public int? Capacity { get; set; }

    public DateTime? RegistrationDeadline { get; set; }

    public bool? BringOwnPc { get; set; }

    [MaxLength(160)]
    public string? SpeakerName { get; set; }

    [MaxLength(2000)]
    public string? Prerequisites { get; set; }

    [Range(1, 50)]
    public int? TeamSize { get; set; }
}

public class UpdateEventRequest : CreateEventRequest
{
}

public class RegisterForEventRequest
{
    [Required, MaxLength(160)]
    public string ConfirmedDisplayName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(200)]
    public string ConfirmedEmail { get; set; } = string.Empty;
}

public class MyEventRegistrationDto
{
    public string EventId { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime StartsAt { get; set; }
    public DateTime RegisteredAt { get; set; }
}
