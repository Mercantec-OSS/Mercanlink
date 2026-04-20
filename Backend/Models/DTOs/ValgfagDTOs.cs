namespace Backend.Models.DTOs;

public class ElectiveEnrollmentParticipantDto
{
    public string DisplayName { get; set; } = string.Empty;

    public DateTime EnrolledAt { get; set; }
}

public class ElectiveEnrollmentsGroupDto
{
    public string ElectiveKey { get; set; } = string.Empty;

    public List<ElectiveEnrollmentParticipantDto> Participants { get; set; } = new();
}

public class EnrollElectiveRequest
{
    public string? ElectiveKey { get; set; }
}
