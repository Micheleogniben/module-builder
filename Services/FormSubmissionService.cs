using Microsoft.JSInterop;
using StaticWebForms.Models;
using System.Text;
using System.Text.Json;

namespace StaticWebForms.Services;

/// <summary>
/// Implementation of IFormSubmissionService for handling form submissions
/// </summary>
public class FormSubmissionService : IFormSubmissionService
{
    private readonly ApiHttpClient _apiClient;
    private readonly IConfigurationService _configService;
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<FormSubmissionService> _logger;
    private string? _apiBaseUrl;

    public FormSubmissionService(
        ApiHttpClient apiClient,
        IConfigurationService configService,
        IJSRuntime jsRuntime,
        ILogger<FormSubmissionService> logger)
    {
        _apiClient = apiClient;
        _configService = configService;
        _jsRuntime = jsRuntime;
        _logger = logger;
        _apiBaseUrl = _configService.GetApiBaseUrl();
        if (!string.IsNullOrEmpty(_apiBaseUrl))
        {
            _apiClient.BaseAddress = new Uri(_apiBaseUrl);
        }
    }

    public async Task<bool> SubmitFormAsync(FormSubmission submission)
    {
        try
        {
            if (string.IsNullOrEmpty(_apiBaseUrl))
            {
                _logger.LogWarning("API base URL not configured. Submission will be skipped.");
                // In development, we can still allow local testing
                // For production, this should be configured
                return false;
            }

            _logger.LogInformation("Submitting form for module {ModuleId} with {AnswerCount} answers", 
                submission.ModuleId, submission.Answers?.Count ?? 0);

            var json = JsonSerializer.Serialize(submission, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            _logger.LogDebug("Form submission payload size: {Size} bytes", json.Length);

            var apiUrl = _apiBaseUrl ?? _apiClient.BaseAddress?.ToString() ?? "";
            if (string.IsNullOrEmpty(apiUrl))
            {
                _logger.LogWarning("API base URL not configured. Submission will be skipped.");
                return false;
            }

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var submitUrl = $"{apiUrl.TrimEnd('/')}/api/submit";
            var response = await _apiClient.PostAsync(submitUrl, content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Form submitted successfully for module {ModuleId}", submission.ModuleId);
            }
            else
            {
                _logger.LogWarning("Form submission failed with status {StatusCode} for module {ModuleId}", 
                    response.StatusCode, submission.ModuleId);
            }

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting form for module {ModuleId}", submission.ModuleId);
            return false;
        }
    }

    public async Task DownloadDocumentAsync(string content, string fileName, string contentType)
    {
        try
        {
            _logger.LogDebug("Preparing download for file {FileName} ({ContentType}, {Size} bytes)", 
                fileName, contentType, content.Length);

            // Convert content to base64 and trigger download via JavaScript
            var bytes = Encoding.UTF8.GetBytes(content);
            var base64 = Convert.ToBase64String(bytes);
            var dataUrl = $"data:{contentType};base64,{base64}";

            await _jsRuntime.InvokeVoidAsync("downloadFile", fileName, dataUrl);
            
            _logger.LogInformation("Download initiated for file {FileName}", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading document {FileName}", fileName);
            throw;
        }
    }
}

