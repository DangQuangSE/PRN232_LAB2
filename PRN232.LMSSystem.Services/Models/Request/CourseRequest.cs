using System.ComponentModel.DataAnnotations;

namespace PRN232.LMSSystem.Services.Models.Request;

/// <summary>Request body for creating or updating a Course record.</summary>
public class CourseRequest
{
    /// <summary>Name/code of the course. Format: SubjectCode_ClassCode_SemesterSuffix.</summary>
    /// <example>PRN232_SE1701_S1</example>
    [Required(ErrorMessage = "CourseName is required.")]
    [MaxLength(100, ErrorMessage = "CourseName must not exceed 100 characters.")]
    public string CourseName { get; set; } = string.Empty;

    /// <summary>ID of the semester this course belongs to. Must reference an existing Semester.</summary>
    /// <example>1</example>
    [Range(1, int.MaxValue, ErrorMessage = "SemesterId must be a positive integer.")]
    public int SemesterId { get; set; }
}
