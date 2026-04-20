using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

public class ElectiveEnrollment : Common
{
    [MaxLength(80)]
    public string ElectiveKey { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public User User { get; set; } = null!;
}
