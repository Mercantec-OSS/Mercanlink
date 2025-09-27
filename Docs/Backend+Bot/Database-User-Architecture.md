# Database User Architecture - Mercantec Space

## Oversigt

Dette dokument beskriver den komplekse user-arkitektur i Mercantec Space systemet, der er designet til at håndtere brugere fra forskellige kilder (Discord, Website, School AD) gennem en centraliseret hovedbruger-model.

## Arkitektur Principper

### 1. Centraliseret User Management
- **Hovedtabel**: `Users` fungerer som den centrale hub for alle brugerdata
- **Separation of Concerns**: Hver brugerkilde har sin egen dedikerede tabel
- **One-to-One Relations**: Hver User har præcis én relation til hver brugerkilde

### 2. Modulær Design
Systemet er opdelt i fire hovedkomponenter:
- **Main User** (`Users`) - Central brugeridentitet
- **Discord User** (`DiscordUsers`) - Discord-specifikke data
- **Website User** (`WebsiteUsers`) - Web-autentificering
- **School AD User** (`SchoolADUsers`) - Skole Active Directory integration

## Database Modeller

### Main User (Users Tabel)
Den centrale bruger-tabel der fungerer som hub for alle andre brugerdata.

```csharp
public class User : Common
{
    public string UserName { get; set; }                    // Unik brugernavn
    public string DiscordUserId { get; set; }              // FK til DiscordUsers
    public string WebsiteUserId { get; set; }              // FK til WebsiteUsers  
    public string SchoolADUserId { get; set; }             // FK til SchoolADUsers
    public List<string> Roles { get; set; }                // Brugerroller (array)
    
    // Navigation Properties
    public SchoolADUser SchoolADUser { get; set; }
    public DiscordUser DiscordUser { get; set; }
    public WebsiteUser WebsiteUser { get; set; }
}
```

**Nøglefunktioner:**
- Unik `UserName` constraint (ikke tom)
- One-to-One relationer til alle brugerkilder
- Roller håndteres som string array
- Arver `Common` properties (Id, CreatedAt, UpdatedAt)

### Discord User (DiscordUsers Tabel)
Håndterer Discord-specifikke data og XP-system.

```csharp
public class DiscordUser : Common
{
    public string? UserName { get; set; }                  // Discord brugernavn
    public string? DiscordId { get; set; }                 // Unik Discord ID
    public string? GlobalName { get; set; }                // Discord global name
    public string? Discriminator { get; set; }             // Discord discriminator
    public string? AvatarUrl { get; set; }                 // Avatar URL
    public string? Nickname { get; set; }                  // Server nickname
    public bool? IsBot { get; set; }                       // Bot status
    public int? PublicFlags { get; set; }                  // Discord public flags
    public DateTime? JoinedAt { get; set; }                // Server join dato
    public bool? IsBoosting { get; set; }                  // Boost status
    public int Experience { get; set; } = 0;               // XP points
    public int Level { get; set; } = 1;                    // Bruger level
    
    public User User { get; set; }                         // Navigation til Main User
}
```

**Nøglefunktioner:**
- Unik `DiscordId` constraint
- XP og Level system integreret
- Nullable properties for fleksibilitet
- Discord API data mapping

### Website User (WebsiteUsers Tabel)
Håndterer web-baseret autentificering og brugerdata.

```csharp
public class WebsiteUser : Common
{
    public string? UserName { get; set; }                  // Web brugernavn
    public string? Email { get; set; }                     // Email adresse
    public string? Password { get; set; }                  // Hashed password
    public bool EmailConfirmed { get; set; }               // Email verifikation status
    
    public User User { get; set; }                         // Navigation til Main User
}
```

**Nøglefunktioner:**
- Unik `Email` constraint (ikke tom)
- Unik `UserName` constraint (ikke tom)
- Email verifikation system
- Password hashing support

### School AD User (SchoolADUsers Tabel)
Integrerer med skolens Active Directory system.

```csharp
public class SchoolADUser : Common
{
    public string? UserName { get; set; }                  // AD brugernavn
    public int? StudentId { get; set; }                    // Elev ID
    
    public User User { get; set; }                         // Navigation til Main User
}
```

**Nøglefunktioner:**
- Student ID mapping
- AD integration support
- Minimal data model for AD data

## Support Tabeller

### Refresh Tokens
Håndterer JWT refresh token management.

```csharp
public class RefreshToken : Common
{
    public string Token { get; set; }                      // Unik token
    public string WebsiteUserId { get; set; }              // FK til WebsiteUser
    public DateTime ExpiresAt { get; set; }                // Udløbsdato
    public bool IsRevoked { get; set; } = false;           // Revokeret status
    public DateTime? RevokedAt { get; set; }               // Revokeret dato
    public string? ReplacedByToken { get; set; }           // Token replacement
    
    public WebsiteUser WebsiteUser { get; set; }           // Navigation
}
```

### User Activity Tracking
Sporer brugeraktivitet og XP tildeling.

```csharp
public class UserActivity : Common
{
    public string DiscordUserId { get; set; }              // FK til DiscordUser
    public string ActivityType { get; set; }               // Aktivitetstype
    public DateTime Timestamp { get; set; }                // Tidsstempel
    public int XPAwarded { get; set; }                     // XP belønning
}

public class UserDailyActivity : Common
{
    public string DiscordUserId { get; set; }              // FK til DiscordUser
    public string ActivityType { get; set; }               // Aktivitetstype
    public DateTime Date { get; set; }                     // Dato
    public int Count { get; set; } = 0;                    // Antal aktiviteter
    public int TotalXPAwarded { get; set; } = 0;           // Total XP
    public DateTime LastActivity { get; set; }             // Sidste aktivitet
}
```

## Database Constraints og Indexes

### Unique Constraints
- `Users.UserName` - Unik brugernavn (ikke tom)
- `Users.DiscordUserId` - Unik Discord relation
- `Users.WebsiteUserId` - Unik Website relation  
- `Users.SchoolADUserId` - Unik School AD relation
- `DiscordUsers.DiscordId` - Unik Discord ID
- `WebsiteUsers.Email` - Unik email (ikke tom)
- `WebsiteUsers.UserName` - Unik brugernavn (ikke tom)
- `RefreshTokens.Token` - Unik refresh token

### Foreign Key Relations
- `Users.DiscordUserId` → `DiscordUsers.Id` (Cascade Delete)
- `Users.WebsiteUserId` → `WebsiteUsers.Id` (Cascade Delete)
- `Users.SchoolADUserId` → `SchoolADUsers.Id` (Cascade Delete)
- `RefreshTokens.WebsiteUserId` → `WebsiteUsers.Id` (Cascade Delete)

## Migration Historie

### Initial Migration (20250821085742_init)
- Oprettede alle hovedtabeller
- Etablerede foreign key relationer
- Oprettede unique constraints og indexes
- Inkluderede `IsActive` felt i DiscordUsers (senere fjernet)

### IsActive Removal (20250911113651_removed_isactive_from_discorduser_model)
- Fjernede `IsActive` felt fra DiscordUsers tabel
- Forenklede Discord user model

## Brugerroller

Systemet understøtter følgende roller defineret i `UserRole` enum:

```csharp
public enum UserRole
{
    Student,        // Standard elev
    Teacher,        // Lærer
    Admin,          // Administrator
    Developer,      // Udvikler
    AdvisoryBoard   // Rådgivende bestyrelse
}
```

## Best Practices

### 1. Data Consistency
- Brug altid Main User som entry point
- Valider data på alle niveauer
- Brug transactions for komplekse operationer

### 2. Performance
- Indexes er optimeret for almindelige queries
- Brug navigation properties for eager loading
- Overvej caching for ofte brugte data

### 3. Security
- Password hashing på application level
- JWT tokens med refresh mechanism
- Role-based access control

### 4. Maintenance
- Regelmæssig cleanup af udløbne tokens
- Monitor XP system performance
- Backup strategi for kritiske data

## Fremtidige Overvejelser

### Mulige Forbedringer
- Audit logging for brugerændringer
- Soft delete funktionalitet
- Bulk import/export funktioner
- Advanced role management
- Multi-tenant support

### Skalering
- Database partitioning strategi
- Read replicas for reporting
- Caching layer implementation
- Microservice decomposition

---

*Dette dokument opdateres løbende med ændringer i systemet.*
