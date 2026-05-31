namespace PRN232.LMSSystem.Services.Models.Response;

/// <summary>
/// Response payload containing authentication tokens.
/// </summary>
public class AuthResponse
{
    /// <summary>
    /// JWT access token to authorize api calls.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Refresh token used to rotating/refreshing the access token.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Life span of the access token in seconds.
    /// </summary>
    public int ExpiresIn { get; set; }
}
