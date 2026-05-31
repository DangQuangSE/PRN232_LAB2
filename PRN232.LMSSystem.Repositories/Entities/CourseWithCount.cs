namespace PRN232.LMSSystem.Repositories.Entities;

public class CourseWithCount
{
    public Course Course { get; set; } = null!;
    public int EnrollmentCount { get; set; }
}
