namespace PRN232.LMSSystem.Repositories.Entities;

public class Enrollment
{
    public int EnrollmentId { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public DateTime EnrollDate { get; set; }
    public string Status { get; set; } = string.Empty;

    public Student Student { get; set; } = null!;
    public Course Course { get; set; } = null!;
}
