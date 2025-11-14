using StaticWebForms.Models;

namespace StaticWebForms.Services;

/// <summary>
/// Service for loading and managing form module configurations
/// </summary>
public interface IModuleService
{
    /// <summary>
    /// Gets a list of all available module IDs
    /// </summary>
    Task<List<string>> GetAvailableModulesAsync();

    /// <summary>
    /// Loads a module configuration by module ID
    /// </summary>
    Task<ModuleConfig?> LoadModuleAsync(string moduleId);
}

