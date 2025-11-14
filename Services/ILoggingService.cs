namespace StaticWebForms.Services;

/// <summary>
/// Service for custom logging functionality, including persistence to localStorage
/// </summary>
public interface ILoggingService
{
    /// <summary>
    /// Logs a message with the specified level
    /// </summary>
    void Log(LogLevel level, string message, Exception? exception = null, Dictionary<string, object>? properties = null);

    /// <summary>
    /// Logs an information message
    /// </summary>
    void LogInformation(string message, Dictionary<string, object>? properties = null);

    /// <summary>
    /// Logs a warning message
    /// </summary>
    void LogWarning(string message, Exception? exception = null, Dictionary<string, object>? properties = null);

    /// <summary>
    /// Logs an error message
    /// </summary>
    void LogError(string message, Exception? exception = null, Dictionary<string, object>? properties = null);

    /// <summary>
    /// Logs a debug message
    /// </summary>
    void LogDebug(string message, Dictionary<string, object>? properties = null);

    /// <summary>
    /// Gets all stored logs
    /// </summary>
    Task<List<LogEntry>> GetLogsAsync(int? maxCount = null);

    /// <summary>
    /// Clears all stored logs
    /// </summary>
    Task ClearLogsAsync();

    /// <summary>
    /// Exports logs as JSON
    /// </summary>
    Task<string> ExportLogsAsJsonAsync();

    /// <summary>
    /// Exports logs as TXT format for download
    /// </summary>
    Task<string> ExportLogsAsTxtAsync();

    /// <summary>
    /// Downloads logs as a TXT file
    /// </summary>
    Task DownloadLogsAsTxtAsync();
}

/// <summary>
/// Represents a log entry
/// </summary>
public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public LogLevel Level { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Exception { get; set; }
    public Dictionary<string, object>? Properties { get; set; }
}

