using Microsoft.JSInterop;
using System.Text;
using System.Text.Json;

namespace StaticWebForms.Services;

/// <summary>
/// Implementation of ILoggingService that stores logs in browser localStorage and reports errors to server
/// </summary>
public class LoggingService : ILoggingService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<LoggingService> _logger;
    private readonly IErrorReportingService? _errorReportingService;
    private const string LogStorageKey = "app_logs";
    private const int MaxLogEntries = 1000; // Maximum number of log entries to keep

    public LoggingService(
        IJSRuntime jsRuntime, 
        ILogger<LoggingService> logger,
        IErrorReportingService? errorReportingService = null)
    {
        _jsRuntime = jsRuntime;
        _logger = logger;
        _errorReportingService = errorReportingService;
    }

    public void Log(LogLevel level, string message, Exception? exception = null, Dictionary<string, object>? properties = null)
    {
        try
        {
            var logEntry = new LogEntry
            {
                Timestamp = DateTime.UtcNow,
                Level = level,
                Message = message,
                Exception = exception?.ToString(),
                Properties = properties
            };

            // Also log to standard logger
            if (exception != null)
            {
                _logger.Log(level, exception, message);
            }
            else
            {
                _logger.Log(level, message);
            }

            // Store in localStorage asynchronously (fire and forget)
            _ = StoreLogEntryAsync(logEntry);

            // Report errors and warnings to server (fire and forget, with rate limiting)
            if ((level == LogLevel.Error || level == LogLevel.Warning) && _errorReportingService != null)
            {
                _ = _errorReportingService.ReportErrorAsync(logEntry);
            }
        }
        catch (Exception ex)
        {
            // Fallback to console if localStorage fails
            _logger.LogError(ex, "Failed to store log entry");
        }
    }

    public void LogInformation(string message, Dictionary<string, object>? properties = null)
    {
        Log(LogLevel.Information, message, null, properties);
    }

    public void LogWarning(string message, Exception? exception = null, Dictionary<string, object>? properties = null)
    {
        Log(LogLevel.Warning, message, exception, properties);
    }

    public void LogError(string message, Exception? exception = null, Dictionary<string, object>? properties = null)
    {
        Log(LogLevel.Error, message, exception, properties);
    }

    public void LogDebug(string message, Dictionary<string, object>? properties = null)
    {
        Log(LogLevel.Debug, message, null, properties);
    }

    private async Task StoreLogEntryAsync(LogEntry logEntry)
    {
        try
        {
            var logs = await GetLogsAsync();
            logs.Insert(0, logEntry); // Add to beginning

            // Keep only the most recent entries
            if (logs.Count > MaxLogEntries)
            {
                logs = logs.Take(MaxLogEntries).ToList();
            }

            var json = JsonSerializer.Serialize(logs);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", LogStorageKey, json);
        }
        catch (Exception ex)
        {
            // Silently fail - we don't want logging to break the app
            _logger.LogError(ex, "Failed to store log entry in localStorage");
        }
    }

    public async Task<List<LogEntry>> GetLogsAsync(int? maxCount = null)
    {
        try
        {
            var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", LogStorageKey);
            if (string.IsNullOrEmpty(json))
            {
                return new List<LogEntry>();
            }

            var logs = JsonSerializer.Deserialize<List<LogEntry>>(json) ?? new List<LogEntry>();
            
            if (maxCount.HasValue)
            {
                return logs.Take(maxCount.Value).ToList();
            }

            return logs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve logs from localStorage");
            return new List<LogEntry>();
        }
    }

    public async Task ClearLogsAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", LogStorageKey);
            LogInformation("Logs cleared");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear logs from localStorage");
        }
    }

    public async Task<string> ExportLogsAsJsonAsync()
    {
        var logs = await GetLogsAsync();
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        return JsonSerializer.Serialize(logs, options);
    }

    public async Task<string> ExportLogsAsTxtAsync()
    {
        var logs = await GetLogsAsync();
        var txt = new System.Text.StringBuilder();
        txt.AppendLine("=== LOG FILE ===");
        txt.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        txt.AppendLine($"Total entries: {logs.Count}");
        txt.AppendLine();
        txt.AppendLine("---");

        foreach (var log in logs)
        {
            txt.AppendLine();
            txt.AppendLine($"[{log.Timestamp:yyyy-MM-dd HH:mm:ss}] {log.Level}");
            txt.AppendLine($"Message: {log.Message}");
            
            if (log.Exception != null)
            {
                txt.AppendLine($"Exception: {log.Exception}");
            }
            
            if (log.Properties != null && log.Properties.Count > 0)
            {
                txt.AppendLine("Properties:");
                foreach (var prop in log.Properties)
                {
                    txt.AppendLine($"  {prop.Key}: {prop.Value}");
                }
            }
            
            txt.AppendLine("---");
        }

        return txt.ToString();
    }

    public async Task DownloadLogsAsTxtAsync()
    {
        try
        {
            var txtContent = await ExportLogsAsTxtAsync();
            var fileName = $"logs_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            
            var bytes = Encoding.UTF8.GetBytes(txtContent);
            var base64 = Convert.ToBase64String(bytes);
            var dataUrl = $"data:text/plain;base64,{base64}";

            await _jsRuntime.InvokeVoidAsync("downloadFile", fileName, dataUrl);
            
            _logger.LogInformation("Logs exported as TXT file: {FileName}", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting logs as TXT");
            throw;
        }
    }
}

