using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

public enum EventRegistrationSource
{
    Web = 0,
    Discord = 1
}

public class EventRegistration : Common
{
    public string EventId { get; set; } = string.Empty;

    public Event Event { get; set; } = null!;

    public string UserId { get; set; } = string.Empty;

    public User User { get; set; } = null!;

    [MaxLength(160)]
    public string ConfirmedDisplayName { get; set; } = string.Empty;

    [MaxLength(200)]
    public string ConfirmedEmail { get; set; } = string.Empty;

    public EventRegistrationSource Source { get; set; } = EventRegistrationSource.Web;
}
