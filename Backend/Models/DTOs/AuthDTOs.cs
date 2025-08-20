namespace Backend.Models.DTOs;

/// <summary>
/// Request for login operation
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Email eller brugernavn
    /// </summary>
    public string EmailOrUsername { get; set; } = string.Empty;
    
    /// <summary>
    /// Password
    /// </summary>
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Request for user registration
/// </summary>
public class RegisterRequest
{
    /// <summary>
    /// Email adresse
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Ønsket brugernavn
    /// </summary>
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// Password
    /// </summary>
    public string Password { get; set; } = string.Empty;
    
    /// <summary>
    /// Valgfri Discord ID til at linke med eksisterende Discord bruger
    /// </summary>
    public string? DiscordId { get; set; }
}

/// <summary>
/// Request for linking Discord account
/// </summary>
public class LinkDiscordRequest
{
    /// <summary>
    /// Discord ID der skal linkes til brugeren
    /// </summary>
    public string DiscordId { get; set; } = string.Empty;
}

/// <summary>
/// Response fra authentication operationer
/// </summary>
public class AuthResponse
{
    /// <summary>
    /// JWT access token
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;
    
    /// <summary>
    /// Refresh token til at få nye access tokens
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;
    
    /// <summary>
    /// Hvornår access token udløber
    /// </summary>
    public DateTime ExpiresAt { get; set; }
    
    /// <summary>
    /// Bruger information
    /// </summary>
    public UserDto User { get; set; } = new();
}

/// <summary>
/// Opdateret UserDto med AD felter
/// </summary>
public class UserDto
{
    /// <summary>
    /// Unik bruger ID (GUID)
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Email adresse
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Brugernavn
    /// </summary>
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// Discord ID (hvis linket)
    /// </summary>
    public string? DiscordId { get; set; }
    
    /// <summary>
    /// Discord global name
    /// </summary>
    public string? GlobalName { get; set; }
    
    /// <summary>
    /// Discord avatar URL
    /// </summary>
    public string? AvatarUrl { get; set; }
    
    /// <summary>
    /// AD felter (GDPR-sikre)
    /// </summary>
    public string? FirstName { get; set; }
    public string? SurnameInitial { get; set; }
    public bool PasswordChanged { get; set; }
    public string? StudentId { get; set; }
    public string? Department { get; set; }
    public string? EmployeeType { get; set; }
    public DateTime? AdCreatedAt { get; set; }
    public DateTime? LastAdSync { get; set; }
    
    /// <summary>
    /// Nuværende experience points
    /// </summary>
    public int Experience { get; set; }
    
    /// <summary>
    /// Nuværende level
    /// </summary>
    public int Level { get; set; }
    
    /// <summary>
    /// Bruger roller
    /// </summary>
    public List<string> Roles { get; set; } = new();
    
    /// <summary>
    /// Om brugeren er aktiv
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Hvornår brugeren sidst er blevet redigeret
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Hvornår brugeren blev oprettet
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Request til opdatering af bruger
/// </summary>
public class UpdateUserRequest
{
    /// <summary>
    /// Nyt brugernavn (optional)
    /// </summary>
    public string? Username { get; set; }
    
    /// <summary>
    /// Ny email (optional)
    /// </summary>
    public string? Email { get; set; }
    
    /// <summary>
    /// Nyt level (optional)
    /// </summary>
    public int? Level { get; set; }
    
    /// <summary>
    /// Ny experience (optional)
    /// </summary>
    public int? Experience { get; set; }
    
    /// <summary>
    /// Nye roller (optional)
    /// </summary>
    public List<string>? Roles { get; set; }
    
    /// <summary>
    /// Om brugeren er aktiv (optional)
    /// </summary>
    public bool? IsActive { get; set; }
} 