using System.ComponentModel.DataAnnotations;

namespace PRN232.LMSSystem.Services.Models.Request;

/// <summary>Request body for creating or updating a Subject record.</summary>
public class SubjectRequest
{
    /// <summary>Unique subject code (uppercase letters and digits only, max 20 chars).</summary>
    /// <example>PRN232</example>
    [Required(ErrorMessage = "SubjectCode is required.")]
    [MaxLength(20, ErrorMessage = "SubjectCode must not exceed 20 characters.")]
    [RegularExpression("^[A-Z0-9]+$", ErrorMessage = "SubjectCode must contain only uppercase letters and digits.")]
    public string SubjectCode { get; set; } = string.Empty;

    /// <summary>Full display name of the subject.</summary>
    /// <example>Advanced Web API Development with ASP.NET Core</example>
    [Required(ErrorMessage = "SubjectName is required.")]
    [MaxLength(200, ErrorMessage = "SubjectName must not exceed 200 characters.")]
    public string SubjectName { get; set; } = string.Empty;

    /// <summary>Number of credits for this subject (1–10).</summary>
    /// <example>3</example>
    [Range(1, 10, ErrorMessage = "Credit must be between 1 and 10.")]
    public int Credit { get; set; }
}
