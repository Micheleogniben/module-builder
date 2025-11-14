using Markdig;
using System.Text.RegularExpressions;

namespace StaticWebForms.Services;

/// <summary>
/// Implementation of IDocumentService for processing templates
/// </summary>
public class DocumentService : IDocumentService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DocumentService> _logger;
    private readonly MarkdownPipeline _markdownPipeline;

    public DocumentService(HttpClient httpClient, ILogger<DocumentService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _markdownPipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();
    }

    public async Task<string?> LoadTemplateAsync(string moduleId)
    {
        try
        {
            // Try Markdown first
            var markdownPath = $"modules/{moduleId}.md";
            try
            {
                var content = await _httpClient.GetStringAsync(markdownPath);
                return content;
            }
            catch
            {
                // If Markdown doesn't exist, try Word placeholder
                // For now, we'll return null - Word processing requires backend
                _logger.LogWarning("Template not found for module {ModuleId}", moduleId);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading template for module {ModuleId}", moduleId);
            return null;
        }
    }

    public string ProcessTemplate(string template, Dictionary<string, object> answers)
    {
        try
        {
            _logger.LogDebug("Processing template with {AnswerCount} answers", answers.Count);
            
            var processed = template;

            // Replace all placeholders in format {{fieldName}}
            var placeholderPattern = new Regex(@"\{\{(\w+)\}\}", RegexOptions.Compiled);
            var matches = placeholderPattern.Matches(template);
            
            _logger.LogDebug("Found {PlaceholderCount} placeholders in template", matches.Count);

            processed = placeholderPattern.Replace(processed, match =>
            {
                var fieldName = match.Groups[1].Value;
                if (answers.TryGetValue(fieldName, out var value))
                {
                    return value?.ToString() ?? string.Empty;
                }
                // If placeholder not found in answers, leave it as is or show placeholder
                _logger.LogWarning("Placeholder {FieldName} not found in answers", fieldName);
                return match.Value;
            });

            _logger.LogInformation("Template processed successfully. Output length: {Length}", processed.Length);
            return processed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing template");
            throw;
        }
    }

    public string MarkdownToHtml(string markdown)
    {
        try
        {
            return Markdown.ToHtml(markdown, _markdownPipeline);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting Markdown to HTML");
            return $"<p>Error processing document: {ex.Message}</p>";
        }
    }
}

