using System.ComponentModel.DataAnnotations;

namespace PRN232.LMSSystem.Services.Models.Request;

/// <summary>Request body for creating or updating a Student record.</summary>
public class StudentRequest
{
    /// <summary>Full name of the student (Vietnamese or English).</summary>
    /// <example>Nguyen Van Anh</example>
    [Required(ErrorMessage = "FullName is required.")]
    [MaxLength(100, ErrorMessage = "FullName must not exceed 100 characters.")]
    public string FullName { get; set; } = string.Empty;

    /// <summary>Student email address. Must be a valid email format.</summary>
    /// <example>student01@fpt.edu.vn</example>
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Email must be a valid email address.")]
    [MaxLength(100, ErrorMessage = "Email must not exceed 100 characters.")]
    public string Email { get; set; } = string.Empty;

    /// <summary>Date of birth in ISO 8601 format (yyyy-MM-dd).</summary>
    /// <example>2002-05-15</example>
    public DateTime DateOfBirth { get; set; }
}
