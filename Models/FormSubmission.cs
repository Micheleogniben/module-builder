namespace StaticWebForms.Models;

/// <summary>
/// Represents a complete form submission ready to be sent to the backend
/// </summary>
public class FormSubmission
{
    public string ModuleId { get; set; } = string.Empty;
    public string ModuleTitle { get; set; } = string.Empty;
    public Dictionary<string, object> Answers { get; set; } = new();
    public string GeneratedDocument { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
}

