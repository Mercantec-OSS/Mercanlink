namespace Backend.Config;

public sealed record ElectiveDefinition(string Key, DateOnly EnrollmentDeadlineUtc);

/// <summary>
/// Gyldige valgfag (slug + sidste dag for tilmelding, inkl. den dag, vurderet i UTC-dato).
/// </summary>
public static class ElectiveDefinitions
{
    public static readonly IReadOnlyList<ElectiveDefinition> All =
    [
        new ElectiveDefinition("deploy-or-die-2026", new DateOnly(2026, 6, 26)),
        new ElectiveDefinition("game-design-2026", new DateOnly(2026, 10, 9)),
        new ElectiveDefinition("machine-learning-2027", new DateOnly(2027, 3, 5)),
    ];

    public static bool IsValidAndOpen(string? electiveKey, DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(electiveKey))
            return false;

        var trimmed = electiveKey.Trim();
        var todayUtc = DateOnly.FromDateTime(utcNow.Date);
        foreach (var def in All)
        {
            if (def.Key == trimmed && todayUtc <= def.EnrollmentDeadlineUtc)
                return true;
        }

        return false;
    }
}
