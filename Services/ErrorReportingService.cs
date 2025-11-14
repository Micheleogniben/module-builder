using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using StaticWebForms.Models;

namespace StaticWebForms.Services;

/// <summary>
/// Service for reporting errors to backend with rate limiting
/// </summary>
public class ErrorReportingService : IErrorReportingService
{
    private readonly ApiHttpClient _apiClient;
    private readonly IConfigurationService _configService;
    private readonly ILogger<ErrorReportingService> _logger;
    private readonly ConcurrentQueue<LogEntry> _errorQueue = new();
    private readonly SemaphoreSlim _rateLimiter = new(1, 1);
    private DateTime _lastReportTime = DateTime.MinValue;
    private const int MinSecondsBetweenReports = 5; // Rate limit: max 1 report ogni 5 secondi
    private const int MaxErrorsPerReport = 10; // Max errori per batch
    private string? _apiBaseUrl;

    public ErrorReportingService(
        ApiHttpClient apiClient,
        IConfigurationService configService,
        ILogger<ErrorReportingService> logger)
    {
        _apiClient = apiClient;
        _configService = configService;
        _logger = logger;
        _apiBaseUrl = _configService.GetApiBaseUrl();
        if (!string.IsNullOrEmpty(_apiBaseUrl))
        {
            _apiClient.BaseAddress = new Uri(_apiBaseUrl);
        }
    }

    public async Task<bool> ReportErrorAsync(LogEntry error)
    {
        // Solo errori e warning vengono inviati
        if (error.Level != LogLevel.Error && error.Level != LogLevel.Warning)
        {
            return false;
        }

        try
        {
            _errorQueue.Enqueue(error);
            return await ProcessErrorQueueAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error queuing error report");
            return false;
        }
    }

    public async Task<bool> ReportErrorsBatchAsync(List<LogEntry> errors)
    {
        var errorsToReport = errors
            .Where(e => e.Level == LogLevel.Error || e.Level == LogLevel.Warning)
            .Take(MaxErrorsPerReport)
            .ToList();

        if (errorsToReport.Count == 0)
        {
            return false;
        }

        foreach (var error in errorsToReport)
        {
            _errorQueue.Enqueue(error);
        }

        return await ProcessErrorQueueAsync();
    }

    private async Task<bool> ProcessErrorQueueAsync()
    {
        await _rateLimiter.WaitAsync();
        try
        {
            // Rate limiting: aspetta se l'ultimo report è stato fatto troppo recentemente
            var timeSinceLastReport = DateTime.UtcNow - _lastReportTime;
            if (timeSinceLastReport.TotalSeconds < MinSecondsBetweenReports)
            {
                var waitTime = MinSecondsBetweenReports - (int)timeSinceLastReport.TotalSeconds;
                await Task.Delay(waitTime * 1000);
            }

            // Raccogli errori dalla coda
            var errorsToSend = new List<LogEntry>();
            while (_errorQueue.TryDequeue(out var error) && errorsToSend.Count < MaxErrorsPerReport)
            {
                errorsToSend.Add(error);
            }

            if (errorsToSend.Count == 0)
            {
                return false;
            }

            // Invia al server
            var apiUrl = _apiBaseUrl ?? _apiClient.BaseAddress?.ToString() ?? "";
            if (string.IsNullOrEmpty(apiUrl))
            {
                _logger.LogWarning("API Base URL not configured, cannot report errors");
                return false;
            }

            var reportUrl = $"{apiUrl.TrimEnd('/')}/api/errors/report";
            
            var report = new ErrorReport
            {
                Errors = errorsToSend,
                Timestamp = DateTime.UtcNow,
                UserAgent = await GetUserAgentAsync()
            };

            var json = JsonSerializer.Serialize(report, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _apiClient.PostAsync(reportUrl, content);

            if (response.IsSuccessStatusCode)
            {
                _lastReportTime = DateTime.UtcNow;
                _logger.LogDebug("Successfully reported {Count} errors to server", errorsToSend.Count);
                return true;
            }
            else
            {
                _logger.LogWarning("Failed to report errors. Status: {StatusCode}", response.StatusCode);
                // Rimetti gli errori in coda per riprovare più tardi
                foreach (var error in errorsToSend)
                {
                    _errorQueue.Enqueue(error);
                }
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing error queue");
            return false;
        }
        finally
        {
            _rateLimiter.Release();
        }
    }

    private Task<string> GetUserAgentAsync()
    {
        // In Blazor WebAssembly, possiamo usare JSInterop per ottenere user agent
        // Per ora ritorniamo una stringa vuota, può essere implementato se necessario
        return Task.FromResult<string>("");
    }
}

/// <summary>
/// Error report model for sending to server
/// </summary>
public class ErrorReport
{
    public List<LogEntry> Errors { get; set; } = new();
    public DateTime Timestamp { get; set; }
    public string? UserAgent { get; set; }
}

