using System.ComponentModel.DataAnnotations;

namespace PRN232.LMSSystem.Services.Models.Request;

/// <summary>Request body for creating or updating an Enrollment record.</summary>
public class EnrollmentRequest
{
    /// <summary>ID of the student to enroll. Must reference an existing Student.</summary>
    /// <example>1</example>
    [Range(1, int.MaxValue, ErrorMessage = "StudentId must be a positive integer.")]
    public int StudentId { get; set; }

    /// <summary>ID of the course to enroll in. Must reference an existing Course.</summary>
    /// <example>5</example>
    [Range(1, int.MaxValue, ErrorMessage = "CourseId must be a positive integer.")]
    public int CourseId { get; set; }

    /// <summary>Date when the student enrolled in the course (ISO 8601 format).</summary>
    /// <example>2024-09-01</example>
    public DateTime EnrollDate { get; set; }

    /// <summary>Enrollment status. Accepted values: Active, Completed, Dropped.</summary>
    /// <example>Active</example>
    [Required(ErrorMessage = "Status is required.")]
    [RegularExpression("^(Active|Completed|Dropped)$", ErrorMessage = "Status must be one of: Active, Completed, Dropped.")]
    public string Status { get; set; } = "Active";
}
