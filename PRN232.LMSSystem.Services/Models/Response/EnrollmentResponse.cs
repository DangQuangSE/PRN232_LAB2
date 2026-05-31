namespace PRN232.LMSSystem.Services.Models.Response;

public class EnrollmentResponse
{
    public int EnrollmentId { get; set; }

    public int StudentId { get; set; }
    public string? StudentName { get; set; }
    public StudentBriefResponse? Student { get; set; }

    public int CourseId { get; set; }
    public string? CourseName { get; set; }
    public CourseBriefResponse? Course { get; set; }

    public DateTime EnrollDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

/// <summary>Enrollment response used inside course context — omits redundant course fields.</summary>
public class EnrollmentOfCourseResponse
{
    public int EnrollmentId { get; set; }
    public int StudentId { get; set; }
    public string? StudentName { get; set; }
    public StudentBriefResponse? Student { get; set; }
    public DateTime EnrollDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

/// <summary>Enrollment response used inside student context — omits redundant student fields.</summary>
public class EnrollmentOfStudentResponse
{
    public int EnrollmentId { get; set; }
    public int CourseId { get; set; }
    public string? CourseName { get; set; }
    public CourseBriefResponse? Course { get; set; }
    public DateTime EnrollDate { get; set; }
    public string Status { get; set; } = string.Empty;
}
