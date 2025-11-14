namespace StaticWebForms.Services;

/// <summary>
/// Wrapper for HttpClient used for static files (modules, templates)
/// This ensures the correct HttpClient is injected into ModuleService and DocumentService
/// </summary>
public class StaticFilesHttpClient : HttpClient
{
    public StaticFilesHttpClient() : base()
    {
    }
}

