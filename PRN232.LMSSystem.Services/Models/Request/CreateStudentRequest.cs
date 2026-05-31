using System.ComponentModel.DataAnnotations;
using PRN232.LMSSystem.Services.Helpers;

namespace PRN232.LMSSystem.Services.Models.Request;

/// <summary>
/// Request model for creating a new student with comprehensive validations.
/// </summary>
public class CreateStudentRequest
{
    /// <summary>
    /// Student code following FPTU Style (e.g. SE19886, CE18793).
    /// </summary>
    /// <example>SE19886</example>
    [Required(ErrorMessage = "StudentCode is required.")]
    [StringLength(10, ErrorMessage = "StudentCode must not exceed 10 characters.")]
    [FptuStudentCode]
    public string StudentCode { get; set; } = string.Empty;

    /// <summary>
    /// Student's full name.
    /// </summary>
    /// <example>Nguyen Van Anh</example>
    [Required(ErrorMessage = "FullName is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "FullName must be between 2 and 100 characters.")]
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Student's email address.
    /// </summary>
    /// <example>student01@fpt.edu.vn</example>
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Email must be a valid email address.")]
    [StringLength(100, ErrorMessage = "Email must not exceed 100 characters.")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Student's phone number.
    /// </summary>
    /// <example>0908123456</example>
    [Required(ErrorMessage = "Phone is required.")]
    [Phone(ErrorMessage = "Phone must be a valid phone format.")]
    [RegularExpression(@"^0\d{9,10}$", ErrorMessage = "Phone must be a valid Vietnamese mobile number (10 or 11 digits starting with 0).")]
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Expected graduation year (range validated).
    /// </summary>
    /// <example>2028</example>
    [Range(2026, 2035, ErrorMessage = "Expected graduation year must be between 2026 and 2035.")]
    public int ExpectedGraduationYear { get; set; } = 2028;

    /// <summary>
    /// Student's date of birth in ISO 8601 format (yyyy-MM-dd).
    /// </summary>
    /// <example>2002-05-15</example>
    [Required(ErrorMessage = "DateOfBirth is required.")]
    public DateTime DateOfBirth { get; set; }
}
