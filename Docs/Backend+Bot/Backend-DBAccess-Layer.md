# DBAccess Layer - Mercantec Space Backend

## Oversigt

DBAccess laget fungerer som data access layer (DAL) i Mercantec Space backend systemet. Det abstraherer database operationer fra business logic og giver en ren interface til data manipulation.

## Arkitektur Rolle

### Position i Systemet
- **Mellem Services og Data lag**
- **Abstraherer Entity Framework kompleksitet**
- **Centraliserer database operationer**
- **Giver konsistent data access patterns**

### Design Principper
- **Single Responsibility**: Hver DBAccess klasse håndterer én entitet
- **Repository Pattern**: Encapsulerer data access logic
- **Dependency Injection**: Injiceret gennem DI container
- **Async/Await**: Alle operationer er asynkrone

## DBAccess Klasser

### 1. UserDBAccess

**Formål**: Håndterer alle bruger-relaterede database operationer

#### Metoder
```csharp
// Query metoder
Task<List<User>> GetAllUsers()
Task<List<User>> GetAllUsersWithBothDiscordAndEmail()
Task<List<User>> GetAllUsersWithDiscordOnly()
Task<List<User>> GetAllUsersWithEmailOnly()
Task<List<User>> GetAllUsersWithDiscord()
Task<List<User>> GetAllUsersWithEmail()
Task<User?> GetUser(string userId)
Task<User?> GetUserFromUsername(string username)

// CRUD metoder
Task AddNewUser(User user)
Task UpdateUser(User user)
Task DeleteUser(User user)

// Utility metoder
Task<int> CountOfUsers()
Task<int> CountOfUsersWithDiscordOnly()
Task<int> CountOfUsersWithEmailOnly()
Task<int> CountOfUsersWithBothDiscordAndEmail()
Task<int> CountOfUsersWithDiscord()
Task<int> CountOfUsersWithEmail()

// Validation metoder
Task<bool> CheckIfUsernameIsInUse(string username, string userId)
Task<List<SchoolADUser>> CheckIfMultipleUsernamesAreInUse(List<string> usernames)
Task<bool> CheckIfEmailIsInUse(string email, string userId)
```

#### Features
- **Eager Loading**: Inkluderer alle relaterede entiteter
- **Filtered Queries**: Specifikke queries for forskellige bruger typer
- **Validation Support**: Metoder til at tjekke data integritet
- **Performance Optimized**: Kun nødvendige data hentes

### 2. AuthDBAccess

**Formål**: Håndterer autentificering-relaterede database operationer

#### Metoder
```csharp
// Authentication metoder
Task<WebsiteUser?> Login(LoginRequest request)
Task<WebsiteUser?> GetWebsiteUser(string websiteUserId)
Task<User?> GetUser(string userId)

// Registration metoder
Task<bool> CheckForExistingUser(RegisterRequest request)
Task<bool> CheckForExistingDiscordIdLink(RegisterRequest request)
Task AddUser(User user)

// User management
Task UpdateUser(User user)
Task DeleteUser(User user)
Task<DiscordUser?> GetDiscordUser(string discordId)
```

#### Features
- **Login Support**: Email/username login validation
- **Registration Validation**: Tjekker for eksisterende brugere
- **Discord Integration**: Håndterer Discord linking
- **User Lifecycle**: Komplet bruger management

### 3. DiscordBotDBAccess

**Formål**: Håndterer Discord bot-relaterede database operationer

#### Features
- **Discord User Management**: Discord-specifikke operationer
- **XP System**: Experience og level management
- **Activity Tracking**: Brugeraktivitet logging
- **Bot Integration**: Discord bot data operations

### 4. DiscordVerificationDBAccess

**Formål**: Håndterer Discord verifikationskoder og processer

#### Features
- **Verification Codes**: Generering og validering
- **Code Management**: Expiry og usage tracking
- **Discord Linking**: Verifikations processer
- **Security**: Secure code generation

### 5. JWTDBAccess

**Formål**: Håndterer JWT refresh token database operationer

#### Features
- **Token Storage**: Refresh token persistence
- **Token Validation**: Token verification
- **Token Cleanup**: Expired token removal
- **Security**: Secure token management

## Implementation Patterns

### 1. Repository Pattern
```csharp
public class UserDBAccess
{
    private readonly ApplicationDbContext _context;

    public UserDBAccess(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<User>> GetAllUsers()
    {
        return await _context.Users
            .Include(u => u.SchoolADUser)
            .Include(u => u.DiscordUser)
            .Include(u => u.WebsiteUser)
            .OrderBy(u => u.CreatedAt)
            .ToListAsync();
    }
}
```

### 2. Eager Loading Pattern
```csharp
// Inkluderer alle relaterede entiteter i ét query
var users = await _context.Users
    .Include(u => u.SchoolADUser)
    .Include(u => u.DiscordUser)
    .Include(u => u.WebsiteUser)
    .ToListAsync();
```

### 3. Filtered Query Pattern
```csharp
// Specifikke queries for forskellige use cases
public async Task<List<User>> GetAllUsersWithDiscordOnly()
{
    return await _context.Users
        .Include(u => u.WebsiteUser)
        .Include(u => u.DiscordUser)
        .Include(u => u.SchoolADUser)
        .Where(u => !string.IsNullOrEmpty(u.DiscordUser.DiscordId) && 
                   string.IsNullOrEmpty(u.WebsiteUser.Email))
        .OrderBy(u => u.CreatedAt)
        .ToListAsync();
}
```

### 4. Validation Pattern
```csharp
// Data validation før operationer
public async Task<bool> CheckIfUsernameIsInUse(string username, string userId)
{
    var existingUser = await _context.WebsiteUsers
        .FirstOrDefaultAsync(u => u.UserName == username && u.Id != userId);
    
    return existingUser != null;
}
```

## Performance Considerations

### 1. Query Optimization
- **Eager Loading**: Inkluderer relaterede data i ét query
- **Selective Loading**: Kun nødvendige felter hentes
- **Indexed Queries**: Bruger database indexes effektivt

### 2. Memory Management
- **Scoped Lifetime**: DBAccess klasser er scoped services
- **Proper Disposal**: Context disposal håndteres automatisk
- **Efficient Queries**: Minimal data transfer

### 3. Database Connection
- **Connection Pooling**: Entity Framework håndterer connection pooling
- **Async Operations**: Alle operationer er asynkrone
- **Transaction Management**: Implicit transaction handling

## Error Handling

### 1. Database Exceptions
```csharp
try
{
    var users = await _context.Users.ToListAsync();
    return users;
}
catch (Exception ex)
{
    // Log error og re-throw eller return null
    _logger.LogError(ex, "Fejl ved hentning af brugere");
    throw;
}
```

### 2. Validation Errors
- **Null Checks**: Validerer input parametre
- **Data Integrity**: Tjekker for constraint violations
- **Business Rules**: Implementerer business validation

## Testing Strategy

### 1. Unit Testing
- **Mock DbContext**: Brug in-memory database
- **Test Data**: Setup test data for hver test
- **Assertion**: Validerer returnerede data

### 2. Integration Testing
- **Real Database**: Test mod faktisk database
- **Transaction Rollback**: Ruller tilbage efter hver test
- **Data Cleanup**: Rydder op efter tests

## Best Practices

### 1. Method Naming
- **Descriptive Names**: Klare, beskrivende metodenavne
- **Consistent Patterns**: Følg samme navngivningskonvention
- **Async Suffix**: Brug Async suffix for async metoder

### 2. Error Handling
- **Logging**: Log alle fejl med kontekst
- **Exception Propagation**: Lad exceptions boble op til service lag
- **Graceful Degradation**: Returner null/empty collections ved fejl

### 3. Performance
- **Eager Loading**: Inkluder relaterede data når nødvendigt
- **Query Optimization**: Brug Include() effektivt
- **Index Usage**: Design queries til at bruge indexes

## Fremtidige Forbedringer

### 1. Caching
- **Query Result Caching**: Cache ofte brugte queries
- **Redis Integration**: Distributed caching
- **Cache Invalidation**: Smart cache invalidation

### 2. Performance Monitoring
- **Query Performance**: Monitor slow queries
- **Connection Pooling**: Monitor connection usage
- **Memory Usage**: Track memory consumption

### 3. Advanced Features
- **Bulk Operations**: Bulk insert/update operations
- **Pagination**: Cursor-based pagination
- **Audit Logging**: Track data changes

---

*Dette dokument opdateres løbende med ændringer i DBAccess laget.*
