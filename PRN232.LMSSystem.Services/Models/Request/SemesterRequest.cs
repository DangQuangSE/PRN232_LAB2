using System.ComponentModel.DataAnnotations;

namespace PRN232.LMSSystem.Services.Models.Request;

/// <summary>Request body for creating or updating a Semester record.</summary>
public class SemesterRequest
{
    /// <summary>Name of the semester (e.g. "Spring 2025", "Summer 2025", "Fall 2025").</summary>
    /// <example>Fall 2025</example>
    [Required(ErrorMessage = "SemesterName is required.")]
    [MaxLength(100, ErrorMessage = "SemesterName must not exceed 100 characters.")]
    public string SemesterName { get; set; } = string.Empty;

    /// <summary>Semester start date in ISO 8601 format (yyyy-MM-dd).</summary>
    /// <example>2025-09-01</example>
    public DateTime StartDate { get; set; }

    /// <summary>Semester end date in ISO 8601 format (yyyy-MM-dd). Must be after StartDate.</summary>
    /// <example>2025-12-31</example>
    public DateTime EndDate { get; set; }
}
