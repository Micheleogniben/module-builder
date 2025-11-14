using StaticWebForms.Models;

namespace StaticWebForms.Services;

/// <summary>
/// Service for reporting errors to the backend server
/// </summary>
public interface IErrorReportingService
{
    /// <summary>
    /// Report an error to the server
    /// </summary>
    Task<bool> ReportErrorAsync(LogEntry error);

    /// <summary>
    /// Report multiple errors in batch
    /// </summary>
    Task<bool> ReportErrorsBatchAsync(List<LogEntry> errors);
}

