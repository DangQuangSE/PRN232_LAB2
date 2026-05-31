namespace PRN232.LMSSystem.Services.Models.Response;

public class SemesterResponse
{
    public int SemesterId { get; set; }
    public string SemesterName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int CourseCount { get; set; }
    public IEnumerable<SemesterCourseResponse>? Courses { get; set; }
}

/// <summary>Embedded semester info without nested collections — used inside other resource responses.</summary>
public class SemesterBriefResponse
{
    public int SemesterId { get; set; }
    public string SemesterName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int CourseCount { get; set; }
}

public class SemesterCourseResponse
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
}
