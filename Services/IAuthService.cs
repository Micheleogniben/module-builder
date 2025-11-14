using StaticWebForms.Models;

namespace StaticWebForms.Services;

/// <summary>
/// Service for handling authentication and authorization
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Check if user is authenticated
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Get current user info
    /// </summary>
    UserInfo? CurrentUser { get; }

    /// <summary>
    /// Login with username and password
    /// </summary>
    Task<LoginResponse> LoginAsync(string username, string password);

    /// <summary>
    /// Logout current user
    /// </summary>
    Task LogoutAsync();

    /// <summary>
    /// Refresh authentication token
    /// </summary>
    Task<bool> RefreshTokenAsync();

    /// <summary>
    /// Get authentication token for API requests
    /// </summary>
    Task<string?> GetTokenAsync();

    /// <summary>
    /// Check if user has admin role
    /// </summary>
    bool IsAdmin();

    /// <summary>
    /// Event fired when authentication state changes
    /// </summary>
    event Action? AuthenticationStateChanged;
}

