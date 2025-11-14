using StaticWebForms.Models;

namespace StaticWebForms.Services;

/// <summary>
/// Service for admin operations (module management)
/// </summary>
public interface IAdminService
{
    /// <summary>
    /// Get list of all modules
    /// </summary>
    Task<List<ModuleInfo>> GetModulesAsync();

    /// <summary>
    /// Get module JSON by ID
    /// </summary>
    Task<string?> GetModuleJsonAsync(string moduleId);

    /// <summary>
    /// Upload/Update module JSON
    /// </summary>
    Task<bool> UploadModuleAsync(string moduleId, string jsonContent);

    /// <summary>
    /// Delete a module
    /// </summary>
    Task<bool> DeleteModuleAsync(string moduleId);

    /// <summary>
    /// Validate module JSON
    /// </summary>
    Task<ValidationResult> ValidateModuleJsonAsync(string jsonContent);
}

/// <summary>
/// Module information for admin console
/// </summary>
public class ModuleInfo
{
    public string ModuleId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int QuestionCount { get; set; }
    public DateTime? LastModified { get; set; }
    public long? FileSize { get; set; }
}

/// <summary>
/// Validation result for module JSON
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

