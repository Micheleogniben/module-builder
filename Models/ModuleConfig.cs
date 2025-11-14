using System.Text.Json.Serialization;

namespace StaticWebForms.Models;

/// <summary>
/// Represents the configuration for a form module loaded from JSON
/// </summary>
public class ModuleConfig
{
    [JsonPropertyName("moduleId")]
    public string ModuleId { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("questions")]
    public List<Question> Questions { get; set; } = new();
}

