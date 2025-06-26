namespace Backend.Models.DTOs;

/// <summary>
/// DTO til bulk import af brugere fra Active Directory
/// </summary>
public class BulkImportRequest
{
    /// <summary>
    /// Liste af brugere der skal importeres
    /// </summary>
    public List<AdUserImportDto> Users { get; set; } = new();
    
    /// <summary>
    /// Standard roller der skal tildeles til alle brugere
    /// </summary>
    public List<string> DefaultRoles { get; set; } = new() { "Student" };
    
    /// <summary>
    /// Standard afdeling hvis ikke angivet
    /// </summary>
    public string? DefaultDepartment { get; set; }
    
    /// <summary>
    /// Om eksisterende brugere skal opdateres
    /// </summary>
    public bool UpdateExisting { get; set; } = false;
    
    /// <summary>
    /// Om der skal sendes velkomst emails
    /// </summary>
    public bool SendWelcomeEmails { get; set; } = false;
}

/// <summary>
/// DTO til en enkelt AD bruger import (GDPR-sikker)
/// </summary>
public class AdUserImportDto
{
    /// <summary>
    /// Brugernavn fra AD (f.eks. alla437h)
    /// </summary>
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// Initial password fra AD
    /// </summary>
    public string InitialPassword { get; set; } = string.Empty;
    
    /// <summary>
    /// Fulde navn som det kommer fra AD - behandles GDPR-sikkert
    /// </summary>
    public string GivenName { get; set; } = string.Empty;
    
    /// <summary>
    /// Efternavn som det kommer fra AD - kun første bogstav gemmes
    /// </summary>
    public string Surname { get; set; } = string.Empty;
    
    /// <summary>
    /// Email adresse hvis tilgængelig
    /// </summary>
    public string? Email { get; set; }
    
    /// <summary>
    /// Student ID fra AD
    /// </summary>
    public string? StudentId { get; set; }
    
    /// <summary>
    /// Afdeling/klasse
    /// </summary>
    public string? Department { get; set; }
    
    /// <summary>
    /// Medarbejder type (Student, Teacher, etc.)
    /// </summary>
    public string? EmployeeType { get; set; }
    
    /// <summary>
    /// Specifikke roller for denne bruger
    /// </summary>
    public List<string>? Roles { get; set; }
}

/// <summary>
/// Resultat af bulk import operation
/// </summary>
public class BulkImportResult
{
    /// <summary>
    /// Antal brugere der blev importeret succesfuldt
    /// </summary>
    public int SuccessCount { get; set; }
    
    /// <summary>
    /// Antal brugere der blev opdateret
    /// </summary>
    public int UpdatedCount { get; set; }
    
    /// <summary>
    /// Antal brugere der fejlede
    /// </summary>
    public int FailedCount { get; set; }
    
    /// <summary>
    /// Antal brugere der blev sprunget over
    /// </summary>
    public int SkippedCount { get; set; }
    
    /// <summary>
    /// Detaljeret resultat for hver bruger
    /// </summary>
    public List<UserImportResult> Results { get; set; } = new();
    
    /// <summary>
    /// Generelle fejl under import
    /// </summary>
    public List<string> Errors { get; set; } = new();
    
    /// <summary>
    /// Tidspunkt for import
    /// </summary>
    public DateTime ImportedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Resultat for en enkelt bruger import
/// </summary>
public class UserImportResult
{
    /// <summary>
    /// Brugernavn der blev forsøgt importeret
    /// </summary>
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// Om import var succesfuld
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Action der blev udført (Created, Updated, Skipped, Failed)
    /// </summary>
    public string Action { get; set; } = string.Empty;
    
    /// <summary>
    /// Besked med detaljer
    /// </summary>
    public string? Message { get; set; }
    
    /// <summary>
    /// Fejl besked hvis import fejlede
    /// </summary>
    public string? Error { get; set; }
    
    /// <summary>
    /// ID på den oprettede/opdaterede bruger
    /// </summary>
    public string? UserId { get; set; }
}

/// <summary>
/// Request til CSV bulk import
/// </summary>
public class CsvBulkImportRequest
{
    /// <summary>
    /// CSV data som string
    /// </summary>
    public string CsvData { get; set; } = string.Empty;
    
    /// <summary>
    /// Mapping af CSV kolonner til bruger felter
    /// </summary>
    public CsvColumnMapping ColumnMapping { get; set; } = new();
    
    /// <summary>
    /// Import indstillinger
    /// </summary>
    public BulkImportSettings Settings { get; set; } = new();
}

/// <summary>
/// Mapping af CSV kolonner
/// </summary>
public class CsvColumnMapping
{
    public int UsernameColumn { get; set; } = 0;
    public int InitialPasswordColumn { get; set; } = 1;
    public int GivenNameColumn { get; set; } = 2;
    public int SurnameColumn { get; set; } = 3;
    public int? EmailColumn { get; set; }
    public int? StudentIdColumn { get; set; }
    public int? DepartmentColumn { get; set; }
    public int? EmployeeTypeColumn { get; set; }
    public bool HasHeader { get; set; } = true;
    public string Delimiter { get; set; } = ",";
}

/// <summary>
/// Indstillinger for bulk import
/// </summary>
public class BulkImportSettings
{
    public List<string> DefaultRoles { get; set; } = new() { "Student" };
    public string? DefaultDepartment { get; set; }
    public bool UpdateExisting { get; set; } = false;
    public bool SendWelcomeEmails { get; set; } = false;
    public bool SkipDuplicates { get; set; } = true;
    public bool ValidateEmails { get; set; } = true;
    public bool RequireStrongPasswords { get; set; } = false;
} 