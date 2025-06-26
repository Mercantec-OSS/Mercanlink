namespace Backend.Models;

public class User : Common
{
    // Basis bruger information
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }

    // Discord integration (optional)
    public string? DiscordId { get; set; }
    public string? GlobalName { get; set; }
    public string? Discriminator { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Nickname { get; set; }
    public bool? IsBot { get; set; }
    public int? PublicFlags { get; set; }
    public DateTime? JoinedAt { get; set; }
    public bool? IsBoosting { get; set; }

    // Active Directory integration (GDPR-sikker)
    public string? FirstName { get; set; }           // Kun første navn fra Given Name
    public string? SurnameInitial { get; set; }      // Kun første bogstav af efternavn
    public string? InitialPassword { get; set; }     // Midlertidigt password fra AD
    public bool PasswordChanged { get; set; } = false; // Om bruger har ændret initial password
    public string? StudentId { get; set; }           // Student ID fra AD/skole system
    public string? Department { get; set; }          // Afdeling/klasse
    public string? EmployeeType { get; set; }        // Student, Teacher, etc.
    public DateTime? AdCreatedAt { get; set; }       // Hvornår bruger blev oprettet i AD
    public DateTime? LastAdSync { get; set; }        // Sidste gang synkroniseret med AD

    // Sikkerhed og roller
    public List<string> Roles { get; set; } = new();

    // Metadata
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    // XP og Level system
    public int Experience { get; set; } = 0;
    public int Level { get; set; } = 1;
}

public enum UserRole
{
    Student,
    Teacher,
    Admin,
    Developer,
    AdvisoryBoard
}
