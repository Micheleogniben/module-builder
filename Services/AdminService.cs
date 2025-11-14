using System.Text;
using System.Text.Json;
using StaticWebForms.Models;

namespace StaticWebForms.Services;

/// <summary>
/// Implementation of IAdminService for module management
/// </summary>
public class AdminService : IAdminService
{
    private readonly HttpClient _apiClient;
    private readonly IAuthService _authService;
    private readonly ILogger<AdminService> _logger;
    private readonly IConfigurationService _configService;
    private string? _apiBaseUrl;

    public AdminService(
        HttpClient apiClient,
        IAuthService authService,
        ILogger<AdminService> logger,
        IConfigurationService configService)
    {
        _apiClient = apiClient;
        _authService = authService;
        _logger = logger;
        _configService = configService;
        _apiBaseUrl = _configService.GetApiBaseUrl();
        if (!string.IsNullOrEmpty(_apiBaseUrl))
        {
            _apiClient.BaseAddress = new Uri(_apiBaseUrl);
        }
    }

    public async Task<List<ModuleInfo>> GetModulesAsync()
    {
        try
        {
            var token = await _authService.GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Cannot get modules: user not authenticated");
                return new List<ModuleInfo>();
            }

            var apiUrl = _apiBaseUrl ?? _apiClient.BaseAddress?.ToString() ?? "";
            
            if (string.IsNullOrEmpty(apiUrl))
            {
                _logger.LogError("API Base URL not configured");
                return new List<ModuleInfo>();
            }
            
            var modulesUrl = $"{apiUrl.TrimEnd('/')}/api/admin/modules";
            var request = new HttpRequestMessage(HttpMethod.Get, modulesUrl);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _apiClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var modules = JsonSerializer.Deserialize<List<ModuleInfo>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return modules ?? new List<ModuleInfo>();
            }

            _logger.LogWarning("Failed to get modules. Status: {StatusCode}", response.StatusCode);
            return new List<ModuleInfo>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting modules");
            return new List<ModuleInfo>();
        }
    }

    public async Task<string?> GetModuleJsonAsync(string moduleId)
    {
        try
        {
            var token = await _authService.GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Cannot get module: user not authenticated");
                return null;
            }

            var apiUrl = _apiBaseUrl ?? _apiClient.BaseAddress?.ToString() ?? "";
            
            if (string.IsNullOrEmpty(apiUrl))
            {
                _logger.LogError("API Base URL not configured");
                return null;
            }
            
            var moduleUrl = $"{apiUrl.TrimEnd('/')}/api/admin/modules/{moduleId}";
            var request = new HttpRequestMessage(HttpMethod.Get, moduleUrl);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _apiClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }

            _logger.LogWarning("Failed to get module {ModuleId}. Status: {StatusCode}", moduleId, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting module {ModuleId}", moduleId);
            return null;
        }
    }

    public async Task<bool> UploadModuleAsync(string moduleId, string jsonContent)
    {
        try
        {
            var token = await _authService.GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Cannot upload module: user not authenticated");
                return false;
            }

            // Validate JSON first
            var validation = await ValidateModuleJsonAsync(jsonContent);
            if (!validation.IsValid)
            {
                _logger.LogWarning("Module JSON validation failed: {Errors}", string.Join(", ", validation.Errors));
                return false;
            }

            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var apiUrl = _apiBaseUrl ?? _apiClient.BaseAddress?.ToString() ?? "";
            
            if (string.IsNullOrEmpty(apiUrl))
            {
                _logger.LogError("API Base URL not configured");
                return false;
            }
            
            var moduleUrl = $"{apiUrl.TrimEnd('/')}/api/admin/modules/{moduleId}";
            var request = new HttpRequestMessage(HttpMethod.Put, moduleUrl)
            {
                Content = content
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _apiClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Module {ModuleId} uploaded successfully", moduleId);
                return true;
            }

            _logger.LogWarning("Failed to upload module {ModuleId}. Status: {StatusCode}", moduleId, response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading module {ModuleId}", moduleId);
            return false;
        }
    }

    public async Task<bool> DeleteModuleAsync(string moduleId)
    {
        try
        {
            var token = await _authService.GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Cannot delete module: user not authenticated");
                return false;
            }

            var apiUrl = _apiBaseUrl ?? _apiClient.BaseAddress?.ToString() ?? "";
            
            if (string.IsNullOrEmpty(apiUrl))
            {
                _logger.LogError("API Base URL not configured");
                return false;
            }
            
            var moduleUrl = $"{apiUrl.TrimEnd('/')}/api/admin/modules/{moduleId}";
            var request = new HttpRequestMessage(HttpMethod.Delete, moduleUrl);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _apiClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Module {ModuleId} deleted successfully", moduleId);
                return true;
            }

            _logger.LogWarning("Failed to delete module {ModuleId}. Status: {StatusCode}", moduleId, response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting module {ModuleId}", moduleId);
            return false;
        }
    }

    public async Task<ValidationResult> ValidateModuleJsonAsync(string jsonContent)
    {
        var result = new ValidationResult { IsValid = true };

        try
        {
            // Try to parse JSON
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var moduleConfig = JsonSerializer.Deserialize<ModuleConfig>(jsonContent, options);
            
            if (moduleConfig == null)
            {
                result.IsValid = false;
                result.Errors.Add("Invalid JSON: Could not parse module configuration");
                return result;
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(moduleConfig.ModuleId))
            {
                result.IsValid = false;
                result.Errors.Add("ModuleId is required");
            }

            if (string.IsNullOrWhiteSpace(moduleConfig.Title))
            {
                result.Warnings.Add("Title is recommended but not required");
            }

            // Validate questions
            if (moduleConfig.Questions == null || moduleConfig.Questions.Count == 0)
            {
                result.Warnings.Add("No questions defined in module");
            }
            else
            {
                var fieldNames = new HashSet<string>();
                foreach (var question in moduleConfig.Questions)
                {
                    if (string.IsNullOrWhiteSpace(question.FieldName))
                    {
                        result.IsValid = false;
                        result.Errors.Add("Question with missing fieldName found");
                    }
                    else if (fieldNames.Contains(question.FieldName))
                    {
                        result.IsValid = false;
                        result.Errors.Add($"Duplicate fieldName: {question.FieldName}");
                    }
                    else
                    {
                        fieldNames.Add(question.FieldName);
                    }

                    if (string.IsNullOrWhiteSpace(question.QuestionType))
                    {
                        result.IsValid = false;
                        result.Errors.Add($"Question {question.FieldName} has missing questionType");
                    }
                }
            }

            return result;
        }
        catch (JsonException ex)
        {
            result.IsValid = false;
            result.Errors.Add($"Invalid JSON format: {ex.Message}");
            return result;
        }
        catch (Exception ex)
        {
            result.IsValid = false;
            result.Errors.Add($"Validation error: {ex.Message}");
            return result;
        }
    }
}

