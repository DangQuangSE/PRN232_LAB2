using System.ComponentModel.DataAnnotations;

namespace PRN232.LMSSystem.Services.Models.Request;

/// <summary>
/// Request payload to refresh expired access tokens.
/// </summary>
public class RefreshTokenRequest
{
    /// <summary>
    /// Valid Refresh Token issued previously.
    /// </summary>
    /// <example>some-long-refresh-token-value</example>
    [Required(ErrorMessage = "RefreshToken is required.")]
    public string RefreshToken { get; set; } = string.Empty;
}
