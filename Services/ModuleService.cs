using System.Text.Json;
using System.Text.Json.Serialization;
using StaticWebForms.Models;

namespace StaticWebForms.Services;

/// <summary>
/// Implementation of IModuleService that loads module configurations from JSON files
/// </summary>
public class ModuleService : IModuleService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ModuleService> _logger;

    public ModuleService(HttpClient httpClient, ILogger<ModuleService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<string>> GetAvailableModulesAsync()
    {
        try
        {
            // Try to load modules from modules.json index file
            try
            {
                var indexContent = await _httpClient.GetStringAsync("json/modules.json");
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var index = JsonSerializer.Deserialize<ModulesIndex>(indexContent, options);
                if (index?.Modules != null && index.Modules.Count > 0)
                {
                    return index.Modules;
                }
            }
            catch
            {
                // If modules.json doesn't exist, fall back to default
                _logger.LogInformation("modules.json not found, using default module list");
            }

            // Fallback to default module list
            return new List<string> { "exampleModule" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available modules");
            return new List<string>();
        }
    }

    private class ModulesIndex
    {
        [JsonPropertyName("modules")]
        public List<string>? Modules { get; set; }
    }

    public async Task<ModuleConfig?> LoadModuleAsync(string moduleId)
    {
        try
        {
            var jsonPath = $"json/{moduleId}.json";
            var jsonContent = await _httpClient.GetStringAsync(jsonPath);
            
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var moduleConfig = JsonSerializer.Deserialize<ModuleConfig>(jsonContent, options);
            return moduleConfig;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error loading module {ModuleId}", moduleId);
            return null;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error parsing module JSON for {ModuleId}", moduleId);
            return null;
        }
    }
}

