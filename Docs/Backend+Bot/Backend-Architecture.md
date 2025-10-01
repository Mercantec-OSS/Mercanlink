# Backend Architecture - Mercantec Space

## Oversigt

Dette dokument beskriver arkitekturen og strukturen af Mercantec Space backend systemet, der er bygget med ASP.NET Core og følger en lagdelt arkitektur med klar separation mellem data access, business logic og API endpoints.

## Arkitektur Principper

### 1. Lagdelt Arkitektur (Layered Architecture)
Systemet er opdelt i fire hovedlag:
- **Controllers** - API endpoints og HTTP request handling
- **Services** - Business logic og application services
- **DBAccess** - Data access layer og database operationer
- **Data** - Entity Framework DbContext og model konfiguration

### 2. Dependency Injection
- Alle services registreres i DI container
- Løs kobling mellem lag gennem interfaces
- Testbarhed og maintainability

### 3. Separation of Concerns
- Hver klasse har et specifikt ansvar
- Database operationer isoleret i DBAccess lag
- Business logic centraliseret i Services lag

## System Struktur

```
Backend/
├── Controllers/          # API Controllers
├── Services/            # Business Logic Services
├── DBAccess/           # Data Access Layer
├── Data/               # Entity Framework & DbContext
├── Models/             # Domain Models & DTOs
├── Discord/            # Discord Bot Integration
├── Config/             # Configuration Classes
├── Jobs/               # Background Jobs
└── Migrations/         # Database Migrations
```

## Lag Detaljer

### 1. Controllers Lag

**Formål**: Håndterer HTTP requests og responses, API endpoints

#### Controllers
- **`UserController`** - Bruger management endpoints
- **`AuthController`** - Autentificering og autorisering
- **`WebhookController`** - Discord webhook handling
- **`WeatherForecastController`** - Test/development endpoints

#### Ansvar
- HTTP request/response mapping
- Input validering
- Error handling og logging
- Authorization checks
- DTO mapping

#### Eksempel - UserController
```csharp
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly UserDBAccess _userDBAccess;
    private readonly ILogger<UserController> _logger;

    [HttpGet]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult<List<UserDto>>> GetAllUsers()
    {
        // Controller logic
    }
}
```

### 2. Services Lag

**Formål**: Business logic, application services, og koordination mellem lag

#### Services
- **`AuthService`** - Autentificering og bruger management
- **`JwtService`** - JWT token generation og validation
- **`DiscordVerificationService`** - Discord verifikations processer

#### Ansvar
- Business logic implementation
- Service orchestration
- Data transformation
- External service integration
- Complex operations coordination

#### Eksempel - AuthService
```csharp
public class AuthService
{
    private readonly AuthDBAccess _authDBAccess;
    private readonly JwtService _jwtService;
    private readonly IOptions<JwtConfig> _jwtConfig;

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        // Business logic for login
    }
}
```

### 3. DBAccess Lag

**Formål**: Data access layer, database operationer, og Entity Framework abstraktion

#### DBAccess Classes
- **`UserDBAccess`** - Bruger database operationer
- **`AuthDBAccess`** - Autentificering database operationer
- **`DiscordBotDBAccess`** - Discord bot database operationer
- **`DiscordVerificationDBAccess`** - Discord verifikation database operationer
- **`JWTDBAccess`** - JWT token database operationer

#### Ansvar
- Database CRUD operationer
- Query optimization
- Data mapping
- Transaction management
- Database error handling

#### Eksempel - UserDBAccess
```csharp
public class UserDBAccess
{
    private readonly ApplicationDbContext _context;

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

### 4. Data Lag

**Formål**: Entity Framework DbContext, model konfiguration, og database mapping

#### Komponenter
- **`ApplicationDbContext`** - EF Core DbContext
- **Model Configuration** - Entity mappings og constraints
- **Migrations** - Database schema management

#### Ansvar
- Database connection management
- Entity configuration
- Relationship mapping
- Migration management
- Database constraints

#### Eksempel - ApplicationDbContext
```csharp
public class ApplicationDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<DiscordUser> DiscordUsers { get; set; }
    public DbSet<WebsiteUser> WebsiteUsers { get; set; }
    public DbSet<SchoolADUser> SchoolADUsers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Entity configurations
    }
}
```

## Discord Integration

### Discord Bot Service
- **`DiscordBotService`** - Hoved Discord bot service
- **`UserService`** - Discord bruger management
- **`XPService`** - XP system management
- **`LevelSystem`** - Level calculation system

### Discord Commands
- **`Commands.cs`** - Discord slash commands implementation

## Configuration Management

### Configuration Classes
- **`JwtConfig`** - JWT token konfiguration
- **`XPConfig`** - XP system konfiguration

### Configuration Sources
- **`appsettings.json`** - Development konfiguration
- **Environment Variables** - Production konfiguration
- **Docker Environment** - Container konfiguration

## Background Jobs

### Cleanup Job
- **`CleanupJob`** - Automatisk cleanup af udløbne tokens og data
- Hosted service der kører i baggrunden
- Konfigurerbar cleanup interval

## Security Implementation

### JWT Authentication
- Access token og refresh token system
- Token validation og refresh
- Role-based authorization

### Password Security
- BCrypt password hashing
- Secure password storage
- Password validation

### CORS Configuration
- Frontend origin whitelist
- Credential support
- Method restrictions

## API Documentation

### Swagger/OpenAPI
- Automatisk API dokumentation
- JWT authentication integration
- XML comment support
- Interactive API testing

### Endpoint Structure
```
/api/auth/*          - Authentication endpoints
/api/user/*          - User management endpoints
/api/webhook/*       - Discord webhook endpoints
/swagger             - API documentation
```

## Database Integration

### Entity Framework Core
- PostgreSQL provider
- Code-first migrations
- Lazy loading disabled
- Explicit includes for performance

### Connection Management
- Environment-based connection strings
- Connection pooling
- Migration automation

## Error Handling & Logging

### Logging Strategy
- Structured logging med ILogger
- Console logging for development
- Error tracking og monitoring

### Exception Handling
- Global exception handling
- Custom error responses
- Logging af alle exceptions

## Performance Considerations

### Database Optimization
- Explicit Include() statements
- Query optimization
- Index utilization
- Connection pooling

### Memory Management
- Scoped services for request lifecycle
- Proper disposal patterns
- Memory-efficient data structures

## Testing Strategy

### Unit Testing
- Service layer testing
- Mock dependencies
- Business logic validation

### Integration Testing
- Database integration tests
- API endpoint testing
- End-to-end scenarios

## Deployment & DevOps

### Docker Support
- Multi-stage Dockerfile
- Environment configuration
- Health checks

### Configuration Management
- Environment-specific settings
- Secret management
- Configuration validation

## Fremtidige Overvejelser

### Skalering
- Microservice decomposition
- Database sharding
- Caching layer implementation

### Performance
- Redis caching
- Database read replicas
- CDN integration

### Monitoring
- Application insights
- Performance metrics
- Health monitoring

---

*Dette dokument opdateres løbende med ændringer i systemet.*
