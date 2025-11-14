namespace StaticWebForms.Services;

/// <summary>
/// Service for processing document templates and replacing placeholders
/// </summary>
public interface IDocumentService
{
    /// <summary>
    /// Loads a template file (Markdown) for a module
    /// </summary>
    Task<string?> LoadTemplateAsync(string moduleId);

    /// <summary>
    /// Replaces placeholders in a template with form answers
    /// </summary>
    string ProcessTemplate(string template, Dictionary<string, object> answers);

    /// <summary>
    /// Converts Markdown to HTML for preview
    /// </summary>
    string MarkdownToHtml(string markdown);
}

