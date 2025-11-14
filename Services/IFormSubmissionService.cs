using StaticWebForms.Models;

namespace StaticWebForms.Services;

/// <summary>
/// Service for preparing and submitting form data to the backend API
/// </summary>
public interface IFormSubmissionService
{
    /// <summary>
    /// Submits form data to the backend API
    /// </summary>
    Task<bool> SubmitFormAsync(FormSubmission submission);

    /// <summary>
    /// Downloads the generated document as a file
    /// </summary>
    Task DownloadDocumentAsync(string content, string fileName, string contentType);
}

