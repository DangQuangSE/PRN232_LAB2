namespace PRN232.LMSSystem.Services.Models.Response;

public class StudentResponse
{
    public int StudentId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public IEnumerable<EnrollmentBriefResponse>? Enrollments { get; set; }
}

/// <summary>Embedded student info without nested collections — used inside other resource responses.</summary>
public class StudentBriefResponse
{
    public int StudentId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
}

public class EnrollmentBriefResponse
{
    public int EnrollmentId { get; set; }
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public DateTime EnrollDate { get; set; }
    public string Status { get; set; } = string.Empty;
}
