using System.ComponentModel.DataAnnotations;

namespace PRN232.LMSSystem.Services.Models.Request;

/// <summary>
/// Credentials needed to authenticate a user.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Registered username.
    /// </summary>
    /// <example>admin</example>
    [Required(ErrorMessage = "Username is required.")]
    [StringLength(50, ErrorMessage = "Username must not exceed 50 characters.")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// User password.
    /// </summary>
    /// <example>123456</example>
    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, ErrorMessage = "Password must not exceed 100 characters.")]
    public string Password { get; set; } = string.Empty;
}
