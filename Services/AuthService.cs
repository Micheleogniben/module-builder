using System.Text;
using System.Text.Json;
using Microsoft.JSInterop;
using StaticWebForms.Models;

namespace StaticWebForms.Services;

/// <summary>
/// Implementation of IAuthService for handling authentication
/// </summary>
public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly HttpClient _apiClient;
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<AuthService> _logger;
    private readonly IConfigurationService _configService;
    private string? _apiBaseUrl;
    private const string TokenStorageKey = "auth_token";
    private const string RefreshTokenStorageKey = "refresh_token";
    private const string UserInfoStorageKey = "user_info";
    private const string TokenExpiryStorageKey = "token_expiry";

    private UserInfo? _currentUser;
    private string? _token;
    private DateTime? _tokenExpiry;

    public bool IsAuthenticated => _token != null && _tokenExpiry.HasValue && _tokenExpiry.Value > DateTime.UtcNow;
    public UserInfo? CurrentUser => _currentUser;

    public event Action? AuthenticationStateChanged;

    public AuthService(
        HttpClient httpClient,
        HttpClient apiClient,
        IJSRuntime jsRuntime,
        ILogger<AuthService> logger,
        IConfigurationService configService)
    {
        _httpClient = httpClient;
        _apiClient = apiClient;
        _jsRuntime = jsRuntime;
        _logger = logger;
        _configService = configService;
        
        // Load API base URL from configuration
        _apiBaseUrl = _configService.GetApiBaseUrl();
        if (!string.IsNullOrEmpty(_apiBaseUrl))
        {
            _apiClient.BaseAddress = new Uri(_apiBaseUrl);
        }
        
        // Load stored authentication state
        _ = LoadStoredAuthAsync();
    }

    private async Task LoadStoredAuthAsync()
    {
        try
        {
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", TokenStorageKey);
            var refreshToken = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", RefreshTokenStorageKey);
            var userInfoJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", UserInfoStorageKey);
            var expiryStr = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", TokenExpiryStorageKey);

            if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(userInfoJson))
            {
                _token = token;
                
                if (DateTime.TryParse(expiryStr, out var expiry))
                {
                    _tokenExpiry = expiry;
                }

                if (!string.IsNullOrEmpty(userInfoJson))
                {
                    _currentUser = JsonSerializer.Deserialize<UserInfo>(userInfoJson);
                }

                // Check if token is expired
                if (_tokenExpiry.HasValue && _tokenExpiry.Value <= DateTime.UtcNow)
                {
                    // Try to refresh token
                    if (!string.IsNullOrEmpty(refreshToken))
                    {
                        await RefreshTokenAsync();
                    }
                    else
                    {
                        await ClearAuthAsync();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading stored authentication");
        }
    }

    public async Task<LoginResponse> LoginAsync(string username, string password)
    {
        try
        {
            _logger.LogInformation("Attempting login for user: {Username}", username);

            var loginRequest = new LoginRequest
            {
                Username = username,
                Password = password
            };

            var json = JsonSerializer.Serialize(loginRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Call API endpoint on Raspberry Pi
            var apiUrl = _apiBaseUrl ?? _apiClient.BaseAddress?.ToString() ?? "";
            
            if (string.IsNullOrEmpty(apiUrl))
            {
                _logger.LogError("API Base URL not configured. Please set ApiBaseUrl in index.html or appsettings.json");
                return new LoginResponse
                {
                    Success = false,
                    ErrorMessage = "API URL non configurata. Contatta l'amministratore."
                };
            }
            
            var loginUrl = $"{apiUrl.TrimEnd('/')}/api/auth/login";
            var response = await _apiClient.PostAsync(loginUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (loginResponse?.Success == true && !string.IsNullOrEmpty(loginResponse.Token))
                {
                    await StoreAuthAsync(loginResponse);
                    _token = loginResponse.Token;
                    _currentUser = loginResponse.User;
                    _tokenExpiry = loginResponse.ExpiresAt;

                    _logger.LogInformation("Login successful for user: {Username}", username);
                    AuthenticationStateChanged?.Invoke();
                    
                    return loginResponse;
                }
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Login failed for user: {Username}. Status: {StatusCode}", username, response.StatusCode);
            
            return new LoginResponse
            {
                Success = false,
                ErrorMessage = $"Login failed: {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user: {Username}", username);
            return new LoginResponse
            {
                Success = false,
                ErrorMessage = $"Error: {ex.Message}"
            };
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            _logger.LogInformation("Logging out user: {Username}", _currentUser?.Username);

            // Call logout endpoint if available
            try
            {
                var apiUrl = _apiBaseUrl ?? _apiClient.BaseAddress?.ToString() ?? "";
                if (!string.IsNullOrEmpty(apiUrl))
                {
                    var logoutUrl = $"{apiUrl.TrimEnd('/')}/api/auth/logout";
                    await _apiClient.PostAsync(logoutUrl, null);
                }
            }
            catch
            {
                // Ignore errors on logout endpoint
            }

            await ClearAuthAsync();
            AuthenticationStateChanged?.Invoke();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
        }
    }

    public async Task<bool> RefreshTokenAsync()
    {
        try
        {
            var refreshToken = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", RefreshTokenStorageKey);
            if (string.IsNullOrEmpty(refreshToken))
            {
                return false;
            }

            var request = new RefreshTokenRequest { RefreshToken = refreshToken };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var apiUrl = _apiBaseUrl ?? _apiClient.BaseAddress?.ToString() ?? "";
            
            if (string.IsNullOrEmpty(apiUrl))
            {
                _logger.LogWarning("API Base URL not configured for refresh token");
                return false;
            }
            
            var refreshUrl = $"{apiUrl.TrimEnd('/')}/api/auth/refresh";
            var response = await _apiClient.PostAsync(refreshUrl, content);
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (loginResponse?.Success == true && !string.IsNullOrEmpty(loginResponse.Token))
                {
                    await StoreAuthAsync(loginResponse);
                    _token = loginResponse.Token;
                    _currentUser = loginResponse.User;
                    _tokenExpiry = loginResponse.ExpiresAt;
                    
                    AuthenticationStateChanged?.Invoke();
                    return true;
                }
            }

            await ClearAuthAsync();
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            await ClearAuthAsync();
            return false;
        }
    }

    public async Task<string?> GetTokenAsync()
    {
        // Check if token is expired and refresh if needed
        if (_tokenExpiry.HasValue && _tokenExpiry.Value <= DateTime.UtcNow.AddMinutes(5))
        {
            await RefreshTokenAsync();
        }

        return _token;
    }

    public bool IsAdmin()
    {
        return _currentUser?.IsAdmin == true || 
               (_currentUser?.Roles?.Contains("Admin") == true) ||
               (_currentUser?.Roles?.Contains("admin") == true);
    }

    private async Task StoreAuthAsync(LoginResponse response)
    {
        try
        {
            if (!string.IsNullOrEmpty(response.Token))
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", TokenStorageKey, response.Token);
            }

            if (!string.IsNullOrEmpty(response.RefreshToken))
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", RefreshTokenStorageKey, response.RefreshToken);
            }

            if (response.User != null)
            {
                var userJson = JsonSerializer.Serialize(response.User);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", UserInfoStorageKey, userJson);
            }

            if (response.ExpiresAt.HasValue)
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", TokenExpiryStorageKey, response.ExpiresAt.Value.ToString("O"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing authentication data");
        }
    }

    private async Task ClearAuthAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", TokenStorageKey);
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", RefreshTokenStorageKey);
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", UserInfoStorageKey);
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", TokenExpiryStorageKey);
            
            _token = null;
            _currentUser = null;
            _tokenExpiry = null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing authentication data");
        }
    }
}

