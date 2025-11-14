using Microsoft.JSInterop;
using System.Text.Json;

namespace StaticWebForms.Services;

/// <summary>
/// Implementation of IConfigurationService that reads from appsettings.json or window.appConfig
/// </summary>
public class ConfigurationService : IConfigurationService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<ConfigurationService> _logger;
    private readonly HttpClient _httpClient;
    private string? _apiBaseUrl;
    private bool _loaded = false;

    public ConfigurationService(IJSRuntime jsRuntime, ILogger<ConfigurationService> logger, HttpClient httpClient)
    {
        _jsRuntime = jsRuntime;
        _logger = logger;
        _httpClient = httpClient;
    }

    public string GetApiBaseUrl()
    {
        if (!_loaded)
        {
            _ = LoadConfigurationAsync();
        }
        return _apiBaseUrl ?? "";
    }

    private async Task LoadConfigurationAsync()
    {
        try
        {
            // First try to get from window.appConfig if set via script tag
            var jsConfig = await _jsRuntime.InvokeAsync<string>("eval", 
                "window.appConfig?.ApiBaseUrl || ''");
            
            if (!string.IsNullOrEmpty(jsConfig))
            {
                _apiBaseUrl = jsConfig;
                _loaded = true;
                _logger.LogInformation("API Base URL loaded from window.appConfig: {Url}", _apiBaseUrl);
                return;
            }

            // Fallback: try to fetch appsettings.json
            try
            {
                var response = await _httpClient.GetAsync("appsettings.json");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var config = JsonSerializer.Deserialize<JsonElement>(json);
                    if (config.TryGetProperty("ApiBaseUrl", out var apiUrl))
                    {
                        _apiBaseUrl = apiUrl.GetString() ?? "";
                        _logger.LogInformation("API Base URL loaded from appsettings.json: {Url}", _apiBaseUrl);
                    }
                }
            }
            catch
            {
                // Ignore - will use empty string
            }

            _loaded = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading configuration");
            _apiBaseUrl = "";
            _loaded = true;
        }
    }
}

