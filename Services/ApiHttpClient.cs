namespace StaticWebForms.Services;

/// <summary>
/// Wrapper for HttpClient used for API calls to Raspberry Pi
/// This ensures the correct HttpClient is injected into API services
/// </summary>
public class ApiHttpClient : HttpClient
{
    public ApiHttpClient() : base()
    {
    }
}

